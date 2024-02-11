#region license
//Copyright 2008 Ritesh Rao 

//Licensed under the Apache License, Version 2.0 (the "License"); 
//you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software 
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
#endregion


using NCommon.Extensions;
using NHibernate;
using System;
using System.Collections.Generic;

namespace NCommon.Data.NHibernate
{
    /// <summary>
    /// Implementation of <see cref="INHSessionResolver"/>.
    /// </summary>
    public class NHSessionResolver : INHSessionResolver
    {
        readonly IDictionary<Type, Guid> _sessionFactoryTypeCache = new Dictionary<Type, Guid>();
        readonly IDictionary<Guid, Func<ISessionFactory>> _sessionFactories = new Dictionary<Guid, Func<ISessionFactory>>();

        /// <summary>
        /// Gets the unique <see cref="ISession"/> key for a type. 
        /// </summary>
        /// <typeparam name="T">The type for which the ObjectContext key should be retrieved.</typeparam>
        /// <returns>A <see cref="Guid"/> representing the unique object context key.</returns>
        public Guid GetSessionKeyFor<T>()
        {
            Guid factorykey;
            if (!_sessionFactoryTypeCache.TryGetValue(typeof(T), out factorykey))
                throw new ArgumentException("No ISessionFactory has been registered for the specified type.");
            return factorykey;
        }

        /// <summary>
        /// Opens a <see cref="ISession"/> instance for a given type.
        /// </summary>
        /// <typeparam name="T">The type for which an <see cref="ISession"/> is returned.</typeparam>
        /// <returns>An instance of <see cref="ISession"/>.</returns>
        public ISession OpenSessionFor<T>()
        {
            var key = GetSessionKeyFor<T>();
            return _sessionFactories[key]().OpenSession();
        }

        /// <summary>
        /// Gets the <see cref="ISessionFactory"/> that can be used to create instances of <see cref="ISession"/>
        /// to query and update the specified type..
        /// </summary>
        /// <typeparam name="T">The type for which an <see cref="ISessionFactory"/> is returned.</typeparam>
        /// <returns>An <see cref="ISessionFactory"/> that can be used to create instances of <see cref="ISession"/>
        /// to query and update the specified type.</returns>
        public ISessionFactory GetFactoryFor<T>()
        {
            Guid factorykey;
            if (!_sessionFactoryTypeCache.TryGetValue(typeof(T), out factorykey))
                throw new ArgumentException("No ISessionFactory has been registered for the specified type.");
            return _sessionFactories[factorykey]();
        }

        /// <summary>
        /// Registers an <see cref="ISessionFactory"/> provider with the resolver.
        /// </summary>
        /// <param name="factoryProvider">A <see cref="Func{T}"/> of type <see cref="ISessionFactory"/>.</param>
        public void RegisterSessionFactoryProvider(Func<ISessionFactory> factoryProvider)
        {
            _ = factoryProvider ?? throw new ArgumentNullException(nameof(factoryProvider), "Expected a non-null Func<ISessionFactory> instance.");

            var key = Guid.NewGuid();
            _sessionFactories.Add(key, factoryProvider);
            //Getting the factory and initializing populating _sessionFactoryTypeCache.
            var factory = factoryProvider();
            var classMappings = factory.GetAllClassMetadata();
            if (classMappings != null && classMappings.Count > 0)
                classMappings.ForEach(map => _sessionFactoryTypeCache
                                                 .Add(map.Value.MappedClass, key));
        }

        /// <summary>
        /// Gets the count of <see cref="ISessionFactory"/> providers registered with the resolver.
        /// </summary>
        public int SessionFactoriesRegistered
        {
            get { return _sessionFactories.Count; }
        }
    }
}