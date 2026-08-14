using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class DialogTemplate
	{
		public string title;

		public DialogTemplateType template;

		public DialogType type;

		[Multiline]
		public string message;

		public Texture2D icon;

		public Texture2D playstationIcon;

		public string[] options;
	}
}
