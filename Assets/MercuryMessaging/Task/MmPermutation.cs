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
using System.Collections.Generic;
using System.Linq;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Permutation class useful for creating MercuryMessaging
    /// MmTaskInfo collections
    /// </summary>
    public static class MmPermutation
    {
        /// <summary>
        /// Given a collection of type IEnumerable, generate permutations of the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Source collection</param>
        /// <param name="length">Length of the collection.</param>
        /// <returns>A 2D collection of the generated permutations.</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => (new T[] { t })).Cast<IEnumerable<T>>();
            }

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Given a collection of type IEnumerable, generate permutations of the collection.
        /// </summary>
        /// <param name="list">Source collection</param>
        /// <returns>A 2D collection of the generated permutations.</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list)
        {
            return GetPermutations(list, list.Count());
        }

        /// <summary>
        /// Given a collection of type IEnumerable, generate permutations of the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Source collection</param>
        /// <returns>A 2D array of the generated permutations.</returns>
        public static T[][] GetPermutationsArray<T>(IEnumerable<T> list)
        {
            return GetPermutations(list, list.Count()).Select(x => x.ToArray()).ToArray();
        }
    }
}