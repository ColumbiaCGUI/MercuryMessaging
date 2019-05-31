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
namespace MercuryMessaging
{
	/// <summary>
	/// The list of support MmRelayNode function calls.
	/// MmMethod only defines values 0-18.
	/// In order to invoke new method types, you can use any number,
	/// without adding it to this list. We suggest you start above 
	/// 1000 to safely avoid clashes with the core of the framework.
	/// </summary>
	public enum MmMethod
	{
		NoOp = 0,
        Initialize,
		SetActive,
		Refresh,
        Switch,
        Complete,
	    TaskInfo,
        Message,
        MessageBool,
        MessageByteArray,
        MessageFloat,
        MessageInt,
        MessageSerializable,
		MessageString,
        MessageTransform,
        MessageTransformList,
        MessageVector3,
        MessageVector4,
        Transform
	}
}