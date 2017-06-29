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

using System.Collections.Generic;
using System.Linq;
using MercuryMessaging.Support;

namespace MercuryMessaging.Task
{

    /// <summary>
    /// This class is useful for creating MmTaskInfos that 
    /// keep repeating but would not iterate through the a MmTaskInfo collection.
    /// </summary>
    /// <typeparam name="T">Specific type of MmTaskInfo to use 
    /// in this repeating task.</typeparam>
	public class MmRepeatingTask<T> where T : MmTaskInfo, new()
    {
		//TODO: Move these data items into a separate for re-use

        /// <summary>
        /// MmRepeatingTasks will iterate through a list of names.
        /// The active name will be stored in <see cref="Task"/>
        /// </summary>
		public List<string> FreeModeNames;
        
        /// <summary>
        /// The name of the active <see cref="FreeModeNames"/> will be at this 
        /// index. 
        /// </summary>
		protected int FreeModeIndex;

        /// <summary>
        /// If active, this object will shuffle the provided 
        /// FreeModeNames.
        /// </summary>
		public bool ShuffleLabels = true;

        /// <summary>
        /// The number of repetitions given to a particular 
        /// name, before the next name in the FreeModeNames list is
        /// selected.
        /// </summary>
        public int FreeModeVizRepeats = 3;

        /// <summary>
        /// Counter for iterating through reptitions.
        /// </summary>
        public int FreeModeVizRepeatIndex = 0;

        /// <summary>
        /// The particular task that is being reused.
        /// You can take a task from a file, or create a new one,
        /// store it here, and simply replace the name stored in the task to
        /// change its behavior as the MmRepeatingTask iterates.
        /// </summary>
        public T Task;

        /// <summary>
        /// Create empty MmRepeatingTask
        /// </summary>
		public MmRepeatingTask()
		{
			FreeModeNames = new List<string>();
            Task = new T();
		}

        /// <summary>
        /// Create MmRepeatingTask with provided labels.
        /// </summary>
        /// <param name="labels">A list of the names of MmResponders to 
        /// use for this repeating task.</param>
		public MmRepeatingTask(List<string> labels)
		{
			FreeModeNames = labels;

			if(ShuffleLabels)
			{
				FreeModeNames = Utilities.Shuffle(FreeModeNames.ToArray()).ToList();
			}

            Task = new T();
			Task.TaskName = FreeModeNames[FreeModeIndex];
		}

        /// <summary>
        /// Iterate through the names in the <see cref="FreeModeNames"/>.
        /// </summary>
		public void IterateRepeatingTask()
		{
			FreeModeVizRepeatIndex++;
			if (FreeModeVizRepeatIndex >= FreeModeVizRepeats)
			{
				FreeModeIndex = ((FreeModeIndex + 1) >= FreeModeNames.Count) ? 0 : (FreeModeIndex + 1);
				Task.TaskName = FreeModeNames[FreeModeIndex];

				FreeModeVizRepeatIndex = 0;
			}
		}
	}
}