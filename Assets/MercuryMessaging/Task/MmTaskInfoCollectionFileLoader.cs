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
using System.Collections.Generic;
using System.IO;
using System.Text;
using MercuryMessaging.Support.Data;
using UnityEngine;
#if NETFX_CORE
using StreamReader = WinRTLegacy.IO.StreamReader;
using StreamWriter = WinRTLegacy.IO.StreamWriter;
#else
using StreamReader = System.IO.StreamReader;
#endif

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Base implementation of ITaskInfoCollectionLoader, specifically for
    /// use in loading from file.
    /// </summary>
    /// <typeparam name="U">Must implement IMmTaskInfo, which contains 
    /// properties for basic data needed by Mercury Task Managers</typeparam>
    [RequireComponent(typeof(MmTaskUserConfigurator))]
	public class MmTaskInfoCollectionFileLoader<U> : MonoBehaviour, ITaskInfoCollectionLoader<U>
        where U : class, IMmTaskInfo, new()
    {
        /// <summary>
        /// Store progress for partially complete task infos
        /// </summary>
        public string PartialDataFile = "PartiallyFinishedTaskSequence.txt";

        /// <summary>
        /// Filename of task collection to be loaded.
        /// </summary>
        public string TaskSeqFilename = "";

        /// <summary>
        /// Current sequence index (global) in the task file - 
        /// value increments across users->blocks->tasks.
        /// </summary>
        public int curSequenceIndex;

        /// <summary>
        /// Reference to user data object, which has user ID and 
        /// directory
        /// </summary>
	    public MmTaskUserConfigurator MmTaskUserData;

        /// <summary>
        /// Auto-grab the MmTaskUserConfigurator
        /// </summary>
        public void Awake()
        {
            MmTaskUserData = GetComponent<MmTaskUserConfigurator>();
        }

        /// <summary>
        /// Prepare task by loading or generating appropriate file.
        /// </summary>
        /// <param name="taskInfos">TaskInfo data structure to fill.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Current Sequence ID after TaskInfos have been loaded.</returns>
        public virtual int PrepareTasks(ref LinkedList<U> taskInfos, int userId)
        {
            int curSequenceIndex = 0;

            curSequenceIndex = LoadPartiallyFinishedTaskSequence();
            LoadTaskSequence(ref taskInfos);

            return curSequenceIndex;
        }

        /// <summary>
        /// If a TaskSequence was interrupted, it is possible to continue
        /// from the last loaded trial before the interruption.
        /// </summary>
        /// <returns>Current sequence index after load.</returns>
        public virtual int LoadPartiallyFinishedTaskSequence()
        {
            if (!File.Exists(Path.Combine(MmTaskUserData.DirPath, PartialDataFile)))
                return 0;

            var reader = new StreamReader(Path.Combine(MmTaskUserData.DirPath, 
                PartialDataFile), 
                Encoding.UTF8);

            string line = reader.ReadLine();
            if (line == null)
            {
                MmLogger.LogError("Partial Sequence File Empty");
                return -1;
            }

            var curSequenceIndex = int.Parse(line.Split(',')[1]);

            reader.Close();

            return curSequenceIndex;
        }

        /// <summary>
        /// Given a collection of task infos, load
        /// the task sequence from file into the collection.
        /// </summary>
        /// <param name="taskInfos">Collection filled by task sequence from
        /// file. Can be empty if file unloaded.</param>
        public virtual void LoadTaskSequence(ref LinkedList<U> taskInfos)
        {
			var filename = MmTaskUserData.BaseDirectory + "/" + TaskSeqFilename;

            try
            {
                using (var sr = new StreamReader(filename, Encoding.UTF8))
                {
                    // Skip header line
                    var line = sr.ReadLine();

                    if (string.IsNullOrEmpty(line)) return;

                    taskInfos = new LinkedList<U>();

                    while ((line = sr.ReadLine()) != null)
                    {
                        var t = new U();
                        t.Parse(line);

                        // Only load current user
                        if (t.UserId != MmTaskUserData.UserId) continue;

                        taskInfos.AddLast(t);
                    }
                }
            }
            catch (Exception e)
            {
                MmLogger.LogError("The file could not be read:");
                MmLogger.LogError(e.Message);
            }
        }
        
        /// <summary>
        /// As we progress through a task sequence, store the partial data value to file.
        /// </summary>
        /// <param name="seqVal">Current Sequence index value.</param>
        public virtual void SaveCurrentTaskSequenceValue(int seqVal)
        {
            var writer = new StreamWriter(Path.Combine(MmTaskUserData.DirPath, PartialDataFile), 
                false, Encoding.Unicode);
            
            writer.WriteLine("Current Sequence ID," + seqVal); 
            //TODO: Check if UserSequence is the right variable to write

            writer.Close();
        }

    }
}