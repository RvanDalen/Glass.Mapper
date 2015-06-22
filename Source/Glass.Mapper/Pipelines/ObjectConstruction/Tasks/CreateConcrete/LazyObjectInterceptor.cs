/*
   Copyright 2012 Michael Edwards
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
*/ 
//-CRE-


using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

namespace Glass.Mapper.Pipelines.ObjectConstruction.Tasks.CreateConcrete
{
    /// <summary>
    /// Class LazyObjectInterceptor
    /// </summary>
    public class LazyObjectInterceptor : IInterceptor
    {
        private readonly ObjectConstructionArgs _args;
        private readonly Dictionary<string, object> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyObjectInterceptor"/> class.
        /// </summary>
        /// <param name="args">The args.</param>
        public LazyObjectInterceptor(ObjectConstructionArgs args)
        {
            _values = new Dictionary<string, object>();
            _args = args;
        }
      
        #region IInterceptor Members

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (!invocation.Method.IsSpecialName ||
                !invocation.Method.Name.StartsWith("get_") && !invocation.Method.Name.StartsWith("set_")) return;

            var accessType = invocation.Method.Name.Substring(0, 4);
            var propertyName = invocation.Method.Name.Substring(4);

            if (accessType.Equals("get_"))
            {
                if (!_values.ContainsKey(propertyName))
                {
                    var typeCreationContext = _args.AbstractTypeCreationContext;
                    if (typeCreationContext == null) return;

                    var typeConfiguration = _args.Configuration;
                    if (typeConfiguration == null) return;

                    var dataMappingContext = _args.Service.CreateDataMappingContext(typeCreationContext, invocation.InvocationTarget);

                    var property = typeConfiguration.Properties.FirstOrDefault(p => p.PropertyInfo.Name.Equals(propertyName));
                    if (property != null)
                    {
                        var result = property.Mapper.MapToProperty(dataMappingContext);
                        _values[propertyName] = result;
                    }
                }

                if (_values.ContainsKey(propertyName))
                    invocation.ReturnValue = _values[propertyName];
            }
            else if (accessType.Equals("set_"))
            {
                _values[propertyName] = invocation.Arguments[0];
            }
        }

        #endregion
    }
}






