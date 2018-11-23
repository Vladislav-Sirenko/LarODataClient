using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class AdapterFactory
    {
        private static readonly string AdapterV4AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string AdapterV4TypeName = typeof(ODataAdapter).ToString();
        private static readonly string ModelAdapterV4TypeName = typeof(ODataModelAdapter).ToString();
        private static readonly string ProtocolVersion = ODataProtocolVersion.V4;

        public IODataModelAdapter CreateModelAdapter(HttpResponseMessage response)
        {
            var loadModelAdapter = GetModelAdapterLoader(ProtocolVersion, response);
            if (loadModelAdapter != null)
                return loadModelAdapter();
            throw new NotSupportedException($"OData protocols {string.Join(",", ProtocolVersion)} are not supported");
        }

        public IODataModelAdapter CreateModelAdapter(string metadataString)
        {
            var loadModelAdapter = GetModelAdapterLoader(ProtocolVersion, metadataString);
            if (loadModelAdapter == null)
                throw new NotSupportedException($"OData protocol {ProtocolVersion} is not supported");

            return loadModelAdapter();
        }

        public Func<ISession, IODataAdapter> CreateAdapter(string metadataString)
        {
            var modelAdapter = CreateModelAdapter(metadataString);

            var loadAdapter = GetAdapterLoader(modelAdapter);
            if (loadAdapter == null)
                throw new NotSupportedException($"OData protocol {modelAdapter.ProtocolVersion} is not supported");

            return loadAdapter;
        }

        private Func<ISession, IODataAdapter> GetAdapterLoader(IODataModelAdapter modelAdapter)
        {
            if (modelAdapter.ProtocolVersion == ODataProtocolVersion.V4)
                return session => LoadAdapter(AdapterV4AssemblyName, AdapterV4TypeName, session, modelAdapter);

            return null;
        }

        private Func<IODataModelAdapter> GetModelAdapterLoader(string protocolVersion, object extraInfo)
        {
            if (protocolVersion == ODataProtocolVersion.V4)
                return () => LoadModelAdapter(AdapterV4AssemblyName, ModelAdapterV4TypeName, protocolVersion, extraInfo);

            return null;
        }

        private IODataModelAdapter LoadModelAdapter(string modelAdapterAssemblyName, string modelAdapterTypeName, params object[] ctorParams)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var ctor = FindAdapterConstructor(assembly, modelAdapterTypeName, ctorParams);
                return ctor.Invoke(ctorParams) as IODataModelAdapter;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Unable to load OData adapter from assembly {modelAdapterAssemblyName}", exception);
            }
        }

        private ConstructorInfo FindAdapterConstructor(Assembly assembly, string modelAdapterTypeName, params object[] ctorParams)
        {
            var constructors = assembly.GetType(modelAdapterTypeName).GetDeclaredConstructors();
            return constructors.Single(x =>
                x.GetParameters().Count() == ctorParams.Count() &&
                x.GetParameters().Last().ParameterType.GetTypeInfo().IsAssignableFrom(ctorParams.Last().GetType().GetTypeInfo()));
        }

        private IODataAdapter LoadAdapter(string adapterAssemblyName, string adapterTypeName, params object[] ctorParams)
        {
            try
            {

                var assembly = Assembly.GetExecutingAssembly();
                var constructors = assembly.GetType(adapterTypeName).GetDeclaredConstructors();
                var ctor = constructors.Single(x =>
                    x.GetParameters().Count() == ctorParams.Count() &&
                    x.GetParameters().Last().ParameterType.IsInstanceOfType(ctorParams.Last()));

                return ctor.Invoke(ctorParams) as IODataAdapter;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Unable to load OData adapter from assembly {adapterAssemblyName}", exception);
            }
        }

    }
}