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
using System.IO;
using UnityEngine;

namespace MercuryMessaging.Support.Data
{
    /// <summary>
    /// Load from and Save to XML files. Data should be fully encapsulated in the object 
    /// passed to the Generic. 
    /// See TransformationDataHandler.cs for an example of how to build on the class
    /// for additional control.
    /// Based on code from http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MmDataHandlerFile : IMmDataHandler
    {
        /// <summary>
        /// File location as a string. Application.DataPath is the default.
        /// </summary>
        public string FileLocation;

        /// <summary>
        /// File name. The default is "yyyy_MM_dd_GenericTitle.xml"
        /// </summary>
        public string FileName;

        /// <summary>
        /// If set, the XML is sought in the resource folder as opposed
        /// to the FileLocation defined.
        /// </summary>
        public bool LoadFromResource = false;

        /// <summary>
        /// Data to be written to file, as written to a string.
        /// </summary>
        public string Data;

        /// <summary>
        /// Writer used in writing to file/stream.
        /// </summary>
        protected StreamWriter Writer;

        /// <summary>
        /// Create an XMLHandler Object.
        /// File location, name, and data can be changed.
        /// </summary>
        public MmDataHandlerFile()
        {
            // Where we want to save and load to and from 
            FileLocation = Application.dataPath;
            FileName = DateTime.Now.ToString("yyyyMMddHHmmss." + GetExtension());
        }

        /// <summary>
        /// Create an MmDataHandlerFile.
        /// </summary>
        /// <param name="fileName">File name - does not need extension.</param>
        public MmDataHandlerFile(string fileName)
        {
            // Where we want to save and load to and from 
            FileLocation = Application.dataPath;
            FileName = fileName + "." + GetExtension();
        }

        /// <summary>
        /// Create an MmDataHandlerFile.
        /// </summary>
        /// <param name="fileName">File name - does not need extension.</param>
        /// <param name="fileLocation">Location of file relative to directory.</param>
        public MmDataHandlerFile(string fileName, string fileLocation)
        {
            // Where we want to save and load to and from 
            FileLocation = fileLocation;
            FileName = fileName + "." + GetExtension();
        }

        /// <summary>
        /// Base file handler creates txt files.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetExtension()
        {
            return "txt";
        }

        /// <summary>
        /// Load data from file.
        /// Supports loading from resource folder and loading directly from the 
        /// project directory.
        /// </summary>
        public virtual void LoadData()
        {
            if (LoadFromResource)
            {
                string fileToLoad = Path.Combine(FileLocation, FileName);
                TextAsset textAsset = (TextAsset)Resources.Load(fileToLoad);
                Data = textAsset.text;
            }
            else
            {
                Load();
            }
        }

        /// <summary>
        /// Open DataHandler file/stream.
        /// </summary>
        /// <param name="overwrite">Should the handler override content 
        /// if file exists.</param>
        /// <returns>New File created?
        /// 0 - No
        /// 1 - Yes
        /// </returns>
        public virtual int Open(bool overwrite = false)
        {
            int newItem = 0;

#if UNITY_STANDALONE || UNITY_EDITOR
            FileInfo t = new FileInfo(Path.Combine(FileLocation, FileName));

            newItem = (!t.Exists || overwrite) ? 1 : 0;

            if (overwrite && t.Exists)
            {
                t.Delete();
                MmLogger.LogApplication("File Deleted: " + FileName);
            }
            Writer = t.AppendText();
            MmLogger.LogApplication("File opened: " + FileName);
#endif

            return newItem;
        }

        /// <summary>
        /// Close DataHandler file/stream.
        /// </summary>
        public virtual void Close()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            Writer.Close();
#else
            Writer.Dispose();
#endif
            MmLogger.LogApplication("File closed: " + FileName);
        }

        /// <summary>
        /// Load file and read into data field member of class.
        /// </summary>
        public virtual void Load()
        {
            StreamReader r = File.OpenText(Path.Combine(FileLocation, FileName));
            Data = r.ReadToEnd();
#if UNITY_STANDALONE || UNITY_EDITOR
            r.Close();
#else
            r.Dispose();
#endif
            MmLogger.LogApplication("File read: " + FileName);
        }

        /// <summary>
        /// Will write header/open tag information.
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public virtual void OpenTag(string[] dataItems)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write the content of dataItems to file.
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public virtual void Write(string[] dataItems)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Will write footer/close tag information.
        /// </summary>
        /// <param name="dataItems">Values to write.</param>
        public virtual void CloseTag(string[] dataItems)
		{
			throw new NotImplementedException();
		}
    }
}

