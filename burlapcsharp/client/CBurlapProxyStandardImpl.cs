/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.hessiancsharp.org
*
* This library is free software; you can redistribute it and/or
* modify it under the terms of the GNU Lesser General Public
* License as published by the Free Software Foundation; either
* version 2.1 of the License, or (at your option) any later version.
*
* This library is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
* Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public
* License along with this library; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
* 
* You can find the GNU Lesser General Public here
* http://www.gnu.org/licenses/lgpl.html
* or in the license.txt file in your source directory.
******************************************************************************************************  
* You can find all contact information on http://www.hessiancsharp.org
******************************************************************************************************
*
*
******************************************************************************************************
* Last change: 2005-12-25
* 2005-12-25 initial class definition by Dimitri Minich.
* ....
******************************************************************************************************
*/
#region NAMESPACES
using Castle.DynamicProxy;
using System;
using System.Reflection;
#endregion

namespace burlapcsharp.client
{
    /// <summary>
    /// Proxy that works with .NET - Remote proxy framework
    /// </summary>
    class CBurlapProxyStandardImpl : IInterceptor, IBurlapProxyStandard
    {
        #region CLASS_FIELDS
		/// <summary>
		/// Interface type, that has to be proxied 
		/// </summary>
		private Type m_proxyType = null;
		// <summary>
		/// Instance to communicate with the Hessian - server
		/// </summary>
		private CBurlapMethodCaller m_methodCaller = null;
		#endregion

		#region CONSTRUCTORS
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="proxyType">Interface type that has to be proxied</param>
		/// <param name="hessianProxyFactory">BurlapProxyFactory - Instance</param>
		/// <param name="uri">Server-Proxy uri</param>
        public CBurlapProxyStandardImpl(Type proxyType, CBurlapProxyFactory burlapProxyFactory, Uri uri)
		{
			this.m_proxyType = proxyType;
            this.m_methodCaller = new CBurlapMethodCaller(burlapProxyFactory, uri);
		}
        /*
        public CBurlapProxyStandardImpl(Type proxyType, CBurlapProxyFactory burlapProxyFactory, Uri uri, string username, string password)
            : base(typeof(IBurlapProxyStandard))
        {
            this.m_proxyType = proxyType;
            this.m_methodCaller = new CBurlapMethodCaller(burlapProxyFactory, uri, username, password);
        }
        */
		#endregion

        public void Intercept(IInvocation invocation)
        {
            var methodInfo = invocation.Method;
            var argumentTypes = invocation.GenericArguments;

            object objReturnValue = null;
            if (methodInfo != null)
            {
                if (methodInfo.Name.Equals("Equals") && argumentTypes != null &&
                    argumentTypes.Length == 1 && argumentTypes[0].IsAssignableFrom((typeof(Object))))
                {
                    Object value = argumentTypes[0];
                    if (value == null)
                    {
                        objReturnValue = false;
                    }
                    else if (value.GetType().Equals(typeof(CBurlapProxy))
                        || value.GetType().IsAssignableFrom(typeof(CBurlapProxy)))
                    {
                        objReturnValue = this.m_methodCaller.URI.Equals(((CBurlapProxy)value).URI);
                    }
                    else
                    {
                        objReturnValue = false;
                    }
                }
                else if (methodInfo.Name.Equals("GetHashCode") && argumentTypes.Length == 0)
                {
                    objReturnValue = this.m_methodCaller.URI.GetHashCode();
                }
                else if (methodInfo.Name.Equals("GetBurlapURL"))
                {
                    objReturnValue = this.m_methodCaller.URI.ToString();
                }
                else if (methodInfo.Name.Equals("ToString") && argumentTypes.Length == 0)
                {
                    objReturnValue = "[BurlapProxy " + this.m_methodCaller.URI + "]";
                }
                else if (methodInfo.Name.Equals("GetType") && argumentTypes.Length == 0)
                {
                    objReturnValue = this.m_proxyType;
                }
                else
                {
                    objReturnValue = this.m_methodCaller.DoBurlapMethodCall(argumentTypes, methodInfo);
                }
            }
            else
            {
                if (methodInfo.Name.Equals("GetType") && (argumentTypes.Length == 0))
                {
                    objReturnValue = this.m_proxyType;
                }
            }

            invocation.ReturnValue = objReturnValue;
        }

        /// <summary>
        /// Checks whether the proxy representing the specified object 
        /// type can be cast to the type represented by the IRemotingTypeInfo interface
        /// </summary>
        /// <param name="fromType">Cast - Type</param>
        /// <param name="obj">Proxy object</param>
        /// <returns>True if the cast type equals or is assingable from the interface type,
        /// wich was used for proxy initialization
        /// </returns>
        public bool CanCastTo(Type fromType, object obj)
        {
            return fromType.Equals(this.m_proxyType) || fromType.IsAssignableFrom(this.m_proxyType);
        }

        /// <summary>
        /// Gets the name of the interface type, 
        /// that has to be proxied 
        /// </summary>
        public string TypeName
        {
            get { return m_proxyType.Name; }
            set { }
        }
    }
}
