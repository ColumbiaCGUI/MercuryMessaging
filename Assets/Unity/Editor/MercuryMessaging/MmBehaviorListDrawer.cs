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

using MercuryMessaging.Support.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MercuryMessaging
{

	/// <summary>
	/// MmRoutingTable drawer.
	/// Note: Based on https://gist.github.com/YuukiTsuchida/84a8f3ebdecf830c277c
	/// </summary>
	[CustomPropertyDrawer(typeof(ReorderableListAttribute))]
	public class MmBehaviorListDrawer : PropertyDrawer {

        /// <summary>
        /// List generated from Editor content.
        /// </summary>
		private ReorderableList list_;

		public void OnEnable()
		{
		}

        /// <summary>
        /// Draw method for ListDrawer.
        /// </summary>
        /// <param name="rect"><see cref="Rect"/></param>
        /// <param name="serializedProperty">Obser</param>
        /// <param name="label"></param>
		public override void OnGUI( Rect rect, SerializedProperty serializedProperty, GUIContent label )
		{
			SerializedProperty listProperty = serializedProperty.FindPropertyRelative( "_list" );
			ReorderableList list = GetList( listProperty );

			float height = 0f;
			for(var i = 0; i < listProperty.arraySize; i++)
			{
				height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i)));
			}
			list.elementHeight = height;
			list.DoList( rect );
		}

        /// <summary>
        /// Extract height of value given serializedProperty.
        /// </summary>
        /// <param name="serializedProperty">Observed property.</param>
        /// <param name="label">Unused here.</param>
        /// <returns>Height as float.</returns>
		public override float GetPropertyHeight( SerializedProperty serializedProperty, GUIContent label )
		{
			SerializedProperty listProperty = serializedProperty.FindPropertyRelative( "_list" );
			return GetList( listProperty ).GetHeight();
		}

        /// <summary>
        /// Create list from that displayed editor.
        /// </summary>
        /// <param name="serializedProperty">The list observed</param>
        /// <returns>Usable reorderable list.</returns>
		private ReorderableList GetList( SerializedProperty serializedProperty )
		{
			if( list_ == null )
			{
				list_ = new ReorderableList( serializedProperty.serializedObject, serializedProperty );
			}

			list_.drawHeaderCallback = (Rect rect) => {  

				var width = rect.width;

				var colWidth = (width - (MmBehaviorListItemDrawer.FieldWidthLevel + MmBehaviorListItemDrawer.FieldWidthClone + MmBehaviorListItemDrawer.FieldWidthTags))/2f;

				rect.width = colWidth;
				EditorGUI.LabelField(rect, new GUIContent("Responder", "MmResponder (Instance or Template)"), EditorStyles.boldLabel);

				rect.x += rect.width;
				rect.width = colWidth;
				EditorGUI.LabelField(rect, "Name", EditorStyles.boldLabel);

				rect.x += rect.width;;
				rect.width = 35;
				EditorGUI.LabelField(rect, new GUIContent("Lvl", "Level (Self, Parent, or Child)"), EditorStyles.boldLabel);

				rect.x += rect.width;;
				rect.width = 20;
				EditorGUI.LabelField(rect, new GUIContent("C?", "Clone?"), EditorStyles.boldLabel);

				rect.x += rect.width;;
				rect.width = 50;
				EditorGUI.LabelField(rect,new GUIContent("Tag(s)", "Multiple choice allowed"), EditorStyles.boldLabel);			
			};

			list_.drawElementCallback =  
					(Rect rect, int index, bool isActive, bool isFocused) => {

				var element = list_.serializedProperty.GetArrayElementAtIndex(index);

				//rect.y += 2;
					
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			};

			return list_;
		}

	}
		
}
