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
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MercuryMessaging.Task
{
	public class MmTaskCollectionDesigner : MonoBehaviour, IMmTaskCollectionDesigner
    {
		public static int SEED = 19790610;

		public int NUM_OF_TASKS = 13;
		public int MAX_USER_COUNT = 100;

		protected System.Random Rnd = new System.Random(SEED);

		protected List<MmTaskInfo> taskSequence;

		/// <summary>
		/// The name of the file.
		/// </summary>
		public string FileName;
		public int NumberOfUnrecordedTasks;
        
        public void Shuffle<T>(ref T[] array)
        {
            var n = array.Length;
            while (n > 1)
            {
                var k = Rnd.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public void Shuffle<T>(ref T[][] array)
        {
            var n = array.Length;
            while (n > 1)
            {
                var k = Rnd.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

	    public void OnValidate()
	    {
            Rnd = new System.Random(SEED);
        }

		public virtual void Generate()
		{}

		public virtual void Save()
		{
			try
			{
                Directory.CreateDirectory(Path.GetDirectoryName(FileName));

                using (var sr = new StreamWriter(
					new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Read),
					Encoding.UTF8))
				{
					sr.WriteLine(taskSequence.First().Headers());

					foreach (var task in taskSequence)
					{
						sr.WriteLine(task);
					}
				}
				Debug.Log("Study file saved to " + FileName);
			}
			catch (Exception e)
			{
				Debug.LogError("The file could not be saved:");
				Debug.LogError(e.Message);
			}
		}

        private bool Within(int h1, int h2, int range)
        {
            return Math.Abs(h1 - h2) < range;
        }
    }
}