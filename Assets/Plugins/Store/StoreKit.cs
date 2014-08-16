﻿/*
 *	The MIT License (MIT)
 *
 *	Copyright (c) 2014 Mateusz Majchrzak
 *
 *	Permission is hereby granted, free of charge, to any person obtaining a copy
 *	of this software and associated documentation files (the "Software"), to deal
 *	in the Software without restriction, including without limitation the rights
 *	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *	copies of the Software, and to permit persons to whom the Software is
 *	furnished to do so, subject to the following conditions:
 *
 *	The above copyright notice and this permission notice shall be included in all
 *	copies or substantial portions of the Software.
 *
 *	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *	SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace Store
{
	/// <summary>
	/// Bridge class for StoreKit and Unity.
	/// </summary>
	public sealed class StoreKit : DataProtocol<StoreKit>, IStore
	{
		#region Native C functions.
		[DllImport("__Internal")]
		private static extern void USKInit(string targetClass);
		[DllImport("__Internal")]
		private static extern bool USKCanMakePayments();
		[DllImport("__Internal")]
		private static extern void USKRequest(string[] identifiers, int length);
		[DllImport("__Internal")]
		private static extern int  USKGetProducts([Out] out IntPtr productsPtr);
		[DllImport("__Internal")]
		private static extern void USKPurchase(string identifier);
		[DllImport("__Internal")]
		private static extern void USKRestore();
		#endregion

		/// <summary>
		/// Gets or sets the delegate.
		/// </summary>
		/// <value>The delegate.</value>
		public IStoreDelegate Delegate { get; set; }

		/// <summary>
		/// Checks that store is available on the current device.
		/// </summary>
		public bool IsAvailable 
		{ 
			get { return USKCanMakePayments(); }
		}

		/// <summary>
		/// Initialize StoreKit instance.
		/// </summary>
		private void Awake()
		{
			USKInit(gameObject.name);
		}

		/// <summary>
		/// Fetch products data form store.
		/// </summary>
		/// <param name="products"></param>
		/// <param name="handler"></param>
		public void Request(IEnumerable<string> products)
		{
			USKRequest(products.ToArray(), products.Count());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="product"></param>
		/// <param name="handler"></param>
		public void Purchase(string productIdentifier)
		{
			USKPurchase(productIdentifier);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Restore()
		{
			USKRestore();
		}

		/// <summary>
		/// 
		/// </summary>
		private void HandleRequestSuccess()
		{
			IntPtr source = IntPtr.Zero;
			int size = USKGetProducts(out source);

			IEnumerable<StoreProduct> shelf = ObjCMarshalArray<StoreProduct>(source, size, new StoreProduct[0]);

			if (Delegate != null)
				Delegate.OnStoreRequestSuccess(shelf);
		}

		/// <summary>
		/// 
		/// </summary>
		private void HandleRequestFailed(string error)
		{
			if (Delegate != null)
				Delegate.OnStoreRequestFailed(error);
		}

		/// <summary>
		/// Handles the transaction success.
		/// </summary>
		/// <param name="productIdentifier">Product identifier.</param>
		private void HandleTransactionSuccess(string identifier)
		{
			if (Delegate != null)
				Delegate.OnStoreTransactionSuccess(identifier);
		}

		/// <summary>
		/// Handles the transaction failed.
		/// </summary>
		private void HandleTransactionFailed(string identifier)
		{
			if (Delegate != null)
				Delegate.OnStoreTransactionFailed(identifier);
		}

		/// <summary>
		/// Handles the transaction restore.
		/// </summary>
		/// <param name="productIdentifier">Product identifier.</param>
		private void HandleTransactionRestore(string identifier)
		{
			if (Delegate != null)
				Delegate.OnStoreTransactionRestore(identifier);
		}
	}
}