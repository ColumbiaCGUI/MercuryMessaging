// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE. 
//  
// =============================================================
// Authors: 
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System;

namespace MercuryMessaging.Support.Data
{
    /// <summary>
    /// MmDataCollectionItems are recording units,
    /// designed to be used with a particular datum.
    /// This class will not store the item to be recorded - instead
    /// it should be passed the instance's print function.
    /// This allows you to fully customize how the print will occur, 
    /// per project, without needing to store additional references/copies.
    /// </summary>
	public class MmDataCollectionItem
	{
        /// <summary>
        /// Name of the value to be recorded.
        /// </summary>
		public string Name;

        /// <summary>
        /// Frequency of data recording for this item.
        /// </summary>
		public MmDataCollectionFreq Freq;
        
        /// <summary>
        /// Print delegate for this item.
        /// Data for the item is not stored with the class,
        /// instead it is treated as a closure.
        /// </summary>
		public Func<string> PrintData;

        /// <summary>
        /// Default constructor.
        /// </summary>
		public MmDataCollectionItem()
		{}


        /// <summary>
        /// Constructor for MmDataCollectionItem.
        /// </summary>
        /// <param name="freq">Frequency of data recording for this item.</param>
        /// <param name="name">Name of data item.</param>
        /// <param name="printData">Print method for item.</param>
		public MmDataCollectionItem(MmDataCollectionFreq freq, 
			string name, Func<string> printData)
		{
			Freq = freq;
			Name = name;
			PrintData = printData;
		}
	}
}