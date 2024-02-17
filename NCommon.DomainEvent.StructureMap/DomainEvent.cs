#region license
//Copyright 2010 Ritesh Rao 

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

using NCommon.State;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NCommon.Events
{
    ///<summary>
    /// DomainEvent class that allowes raising domain events from domain entities and allow registring
    /// custom callbacks to execute when a <see cref="IDomainEvent"/> is raised.
    ///</summary>
    public static class DomainEvent
    {
        const string CallbackListKey = "DomainEvent.Callbacks";

        public static IContainer Container { get; private set; }
        
        /// <summary>
        /// You must initialize this class before use. 
        /// </summary>
        /// <param name="container">Your initialized StructureMap IContainer.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Initialize(IContainer container) 
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        ///<summary>
        /// Registers a callback to be called when a domain event is raised.
        ///</summary>
        ///<param name="callback">An <see cref="Action{T}"/> to be invoked.</param>
        ///<typeparam name="T">The domain event that the callback is registered to handle.</typeparam>
        public static void RegisterCallback<T>(Action<T> callback) where T : IDomainEvent
        {
            var state = Container.GetInstance<IState>();
            var callbacks = state.Local.Get<IList<Delegate>>(CallbackListKey);
            if (callbacks == null)
            {
                callbacks = new List<Delegate>();
                state.Local.Put(CallbackListKey, callbacks);
            }
            callbacks.Add(callback);
        }

        ///<summary>
        /// Clears all callbacks registered on the current thread.
        ///</summary>
        public static void ClearCallbacks()
        {
            var state = Container.GetInstance<IState>();
            state.Application.Remove<IList<Delegate>>(CallbackListKey);
        }

        ///<summary>
        /// Raises a <see cref="IDomainEvent"/>.
        ///</summary>
        ///<param name="event">A instance <see cref="IDomainEvent"/> to raise.</param>
        ///<typeparam name="T">A type implementing <see cref="IDomainEvent"/></typeparam>
        public static void Raise<T>(T @event) where T : IDomainEvent
        {
            var state = Container.GetInstance<IState>();
            var handlers = Container.GetAllInstances<IHandles<T>>();
            if (handlers != null)
                handlers.ToList().ForEach(x => x.Handle(@event));

            var callbacks = state.Local.Get<IList<Delegate>>(CallbackListKey);
            if (callbacks != null && callbacks.Count > 0)
                callbacks.OfType<Action<T>>().ToList().ForEach(x => x(@event));
        }
    }
}