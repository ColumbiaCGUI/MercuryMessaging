// Copyright (c) 2017, Columbia University 
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MercuryMessaging.Support.Data
{
    /// <summary>
    /// The MmDataCollector stores a collection of MmDataCollectionItems.
    /// If an MmDataCollector is given a "EveryFrame" update frequency,
    /// the items in its collection will write to file in this object's LateUpdate method.
    /// </summary>
    public class MmDataCollector : MonoBehaviour
    {
        /// <summary>
        /// Collection of MmDataCollectionItem <see cref="MmDataCollectionItem"/>
        /// </summary>
        protected Dictionary<string, MmDataCollectionItem> dataItems =
            new Dictionary<string, MmDataCollectionItem>();

        /// <summary>
        /// MmDataCollectors record at a the specified frequency.
        /// <see cref="MmDataCollectionFreq"/>
        /// </summary>
		public MmDataCollectionFreq MmDataCollectionFreq;

        /// <summary>
        /// Designated output format for the object.
        /// <see cref="MmDataCollectionOutputType"/>
        /// </summary>
        public MmDataCollectionOutputType OutputType;

        /// <summary>
        /// Name of file - extension not needed.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Location of file. 
        /// </summary>
        public string FileLocation;

        /// <summary>
        /// Handle to MmDataHandler - determined by system.
        /// </summary>
        private IMmDataHandler dataHandler;

        /// <summary>
        /// When enabled, the DataCollector will be allowed to write 
        /// to file or stream.
        /// </summary>
        public bool AllowWrite = false;

        /// <summary>
        /// Creates an empty collection of MmDataCollectionItems.
        /// </summary>
        public void Awake()
        {
            dataItems = new Dictionary<string, MmDataCollectionItem>();
        }

        /// <summary>
        /// Create data handler for this object's MmDataCollectionItems.
        /// </summary>
        /// <param name="fileLocation">Location of file</param>
        /// <param name="fileName">Filename without extension.</param>
        /// <returns>New DataHandler File/Stream created? 
        /// 0 - No
        /// 1 - Yes
        /// </returns>
        public virtual int CreateDataHandler(string fileLocation,
            string fileName)
        {
            FileLocation = fileLocation;
            FileName = fileName;

            switch (OutputType)
            {
                case MmDataCollectionOutputType.CSV:
                    dataHandler = new MmDataHandlerCsv(FileName, FileLocation);
                    break;
                case MmDataCollectionOutputType.XML:
                    dataHandler = new MmDataHandlerXml(FileName, FileLocation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int newItem = OpenDataHandler();

            AllowWrite = true;

            return newItem;
        }

        /// <summary>
        /// Open DataHandler file/stream.
        /// </summary>
        /// <returns>New File/Stream created? 0 - No, 1 - Yes</returns>
        public virtual int OpenDataHandler()
        {
            return dataHandler.Open();
        }

        /// <summary>
        /// Close DataHandler file/stream.
        /// </summary>
        public virtual void CloseDataHandler()
        {
            dataHandler.Close();
        }

        /// <summary>
        /// If this object has MmDataCollectionFreq set to EveryFrame,
        /// items will get written during LateUpdate.
        /// </summary>
        public void LateUpdate()
        {
			if(MmDataCollectionFreq == MmDataCollectionFreq.EveryFrame)
            	Write();
        }

        /// <summary>
        /// Will write header/open tag information through the datahandler's 
        /// particular implementation.
        /// </summary>
		public void OpenTag ()
		{
			dataHandler.OpenTag (dataItems.Values.Select(x => x.Name).ToArray());
		}

        /// <summary>
        /// Write the content of MmDataCollectionItems using their
        /// PrintData methods.
        /// </summary>
        public void Write()
        {
            //TODO: Evaluate LINQ performance penalty here
            if(AllowWrite)
                dataHandler.Write(dataItems.Values.Select(x => x.PrintData.Invoke()).ToArray());            
        }

        /// <summary>
        /// Will write footer/close tag information through the datahandler's 
        /// particular implementation.
        /// </summary>
		public void CloseTag ()
		{
			dataHandler.CloseTag (dataItems.Values.Select(x => x.Name).ToArray());
		}

        /// <summary>
        /// Add an MmDataCollectionItem to the item collection.
        /// </summary>
        /// <param name="name">Name of MmDataCollectionItem</param>
        /// <param name="printData">Print Delegate</param>
		public void Add(string name, Func<string> printData)
		{
			MmDataCollectionItem mmDataItem = 
				new MmDataCollectionItem (MmDataCollectionFreq, name, printData);

			dataItems.Add (name, mmDataItem);
		}

        /// <summary>
        /// Clear the collection of MmDataCollectionItems.
        /// </summary>
        public void Clear()
        {
            dataItems.Clear();
        }
    }
}