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

namespace RCommon.StateStorage
{
    /// <summary>
    /// Default implementation of <see cref="IStateStorage"/>.
    /// </summary>
    public class StateStorageWrapper : IStateStorage
    {
        readonly IApplicationState _applicationState;
        readonly IContextState _localState;
        readonly ISessionState _sessionState;

        /// <summary>
        /// Default Constructor.
        /// Creates a new instance of <see cref="IStateStorage"/> class.
        /// </summary>
        /// <param name="applicationState">An instance of <see cref="IApplicationState"/> that is used to store
        /// application state data.</param>
        /// <param name="localState">An instance of <see cref="IContextState"/> that is used to store local
        /// state data.</param>
        /// <param name="sessionState">An instance of <see cref="ISessionState"/> that is used to store session
        /// state data.</param>
        
        public StateStorageWrapper(
            IApplicationState applicationState, 
            IContextState localState, 
            ISessionState sessionState)
        {
            _applicationState = applicationState;
            _localState = localState;
            _sessionState = sessionState;
        }

        /// <summary>
        /// Gets the application specific state.
        /// </summary>
        public IApplicationState Application
        {
            get { return _applicationState; }
        }

        /// <summary>
        /// Gets the thread local / request local specific state.
        /// </summary>
        public IContextState Local
        {
            get { return _localState; }
        }

        /// <summary>
        /// Gets the session specific state.
        /// </summary>
        public ISessionState Session
        {
            get { return _sessionState; }
        }
    }
}