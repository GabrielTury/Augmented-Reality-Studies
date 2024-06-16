﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AnythingWorld.GLTFUtility {
	[Serializable]
	public class GLTFExtrasProcessor
	{
		public virtual void ProcessExtras(GameObject importedObject, AnimationClip[] animations, JObject extras)
		{
		}
	}
}