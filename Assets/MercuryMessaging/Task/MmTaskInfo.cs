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

using UnityEngine.Networking;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// MmTaskInfo contains basic information for tasks in the Mercury environment.
    /// </summary>
    public class MmTaskInfo : IMmTaskInfo, IMmSerializable
    {
        /// <summary>
        /// Default constructor, all members set to default values.
        /// </summary>
        public MmTaskInfo()
        {
        }

        // TODO: Push the field "practice" to down to inheriting class (e.g., OrTaskInfo)
        /// <summary>
        /// Create an MmTaskInfo by passing all parameters
        /// </summary>
        /// <param name="recId">Record ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="seq">Sequence ID</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="doNotRecordData">Do not record this task.</param>
        /// <param name="taskName">Name of the task</param>
        public MmTaskInfo(int recId, int userId, int seq, int taskId, bool doNotRecordData, string taskName)
        {
            RecordId = recId;
            UserId = userId;
            UserSequence = seq;
            TaskId = taskId;
            DoNotRecordData = doNotRecordData;
            TaskName = taskName;
        }

        /// <summary>
        /// Construct MmTaskInfo by duplicating another task.
        /// </summary>
        /// <param name="orig">Original task</param>
        public MmTaskInfo(IMmTaskInfo orig)
        {
            RecordId = orig.RecordId;
            UserId = orig.UserId;
            UserSequence = orig.UserSequence;
            TaskId = orig.TaskId;
            DoNotRecordData = orig.DoNotRecordData;
            TaskName = orig.TaskName;
        }

        /// <summary>
        /// Generate member values by parsing string.
        /// </summary>
        /// <param name="str"></param>
        public virtual void Parse(string str)
        {
            var words = str.Split(',');
            
            RecordId = int.Parse(words[0]);
            UserId = int.Parse(words[1]);
            UserSequence = int.Parse(words[2]);
            TaskId = int.Parse(words[3]);
            DoNotRecordData = bool.Parse(words[4]);
            TaskName = words[5];
        }

        /// <summary>
        /// Headers to use when writing task to file or stream.
        /// </summary>
        /// <returns></returns>
        public virtual string Headers()
        {
			return "Rec.TaskId,User.TaskId,Trial.UserSequence,Trial.TaskId,DoNotRecordData,Condition";
        }

        /// <summary>
        /// To String generates string of task with following order:
        /// RecordID, UserID, SeqID, TaskID, DoNotRecord, TaskName
        /// </summary>
        /// <returns>String containing base task info.</returns>
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}", RecordId, UserId, UserSequence, TaskId, DoNotRecordData, TaskName);
        }

        /// <summary>
        /// Duplicate the task info.
        /// </summary>
        /// <returns>Copy of MmTaskInfo</returns>
        public virtual IMmSerializable Copy()
        {
            return new MmTaskInfo(this);
        }

        /// <summary>
        /// Deserialize the task info from serialized form.
        /// </summary>
        /// <param name="reader">UNET deserializer.</param>
        public virtual void Deserialize(NetworkReader reader)
        {
            RecordId = reader.ReadInt32();
            UserId = reader.ReadInt32();
            UserSequence = reader.ReadInt32();
            TaskId = reader.ReadInt32();
            DoNotRecordData = reader.ReadBoolean();
            TaskName = reader.ReadString();
        }

        /// <summary>
        /// Serialize the task info into serialized form.
        /// </summary>
        /// <param name="writer">UNET serializer.</param>
        public virtual void Serialize(NetworkWriter writer)
        {
            writer.Write(RecordId);
            writer.Write(UserId);
            writer.Write(UserSequence);
            writer.Write(TaskId);
            writer.Write(DoNotRecordData);
            writer.Write(TaskName);
        }

        /// <summary>
        /// Record ID
        /// </summary>
        public int RecordId { get; set; }
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Sequence ID, given a particular user
        /// </summary>
        public int UserSequence { get; set; }
        /// <summary>
        /// TaskID, useful for blocking tasks.
        /// </summary>
        public int TaskId { get; set; }
        /// <summary>
        /// Name of task.
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// Indicate whether you want to (default) 
        /// avoid recording a particular task.
        /// </summary>
        public bool DoNotRecordData { get; set; }
    }
}