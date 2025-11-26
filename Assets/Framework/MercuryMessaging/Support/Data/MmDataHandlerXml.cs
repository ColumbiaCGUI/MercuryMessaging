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
namespace MercuryMessaging.Support.Data
{
    /// <summary>
    /// XML implementation of the IMmDataHandler.
    /// </summary>
    public class MmDataHandlerXml : MmDataHandlerFile
    {
        /// <summary>
        /// Default construction.
        /// </summary>
        public MmDataHandlerXml()
        {}

        /// <summary>
        /// Create an MmDataHandlerXml
        /// </summary>
        /// <param name="fileName">Filename - does not require extension.</param>
        public MmDataHandlerXml(string fileName) : base(fileName)
        {}

        /// <summary>
        /// Create an MmDataHandlerXml
        /// </summary>
        /// <param name="fileName">Filename - does not require extension.</param>
        /// <param name="fileLocation">Qualified path relative to project directory.</param>
        public MmDataHandlerXml(string fileName, string fileLocation) : base(fileName, fileLocation)
        {}

        /// <summary>
        /// Returns .xml extension for use by base class' file handler.
        /// </summary>
        /// <returns>Returns .xml extension.</returns>
        protected override string GetExtension()
        {
            return "xml";
        }

        /// <summary>
        /// Will write XML open tag information.  
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public override void OpenTag(string[] dataItems)
        {
        	//TODO: Implement this
           // Open root tag
        }

        /// <summary>
        /// Write the body content in XML form.
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public override void Write(string[] dataItems)
        {
            //TODO: Implement this
            // Foreach item in dataItems, envelop it with the corresponding variable name (coming from setNames)
        }

        /// <summary>
        /// Will write XML close tag information.  
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public override void CloseTag(string[] dataItems)
		{
			//TODO: Implement this
			// Close root tag. 
		}

        /// <summary>
        /// Load the data for use by deserializer.
        /// </summary>
        public override void Load()
        {
            base.Load();

            MmLogger.LogApplication("Serializing XML");

            // TODO: Serialize XML here
        }        
    }
}

