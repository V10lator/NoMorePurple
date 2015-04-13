// This is written in C#

using System;
using System.IO;
using System.Reflection;
using ICities;
using UnityEngine;

/*
* Standard mod declaration.  Check the cities sklyines wiki for more information.
*/
namespace NoMorePurple
{

    public class NoMorePurple : LoadingExtensionBase, IUserMod 
    {
		OptionWindow optionWindow = null;

        public string Name {
			get {
				if (optionWindow == null) {
					GameObject go = new GameObject ("No More Purples OptionWindow");
					go.AddComponent<OptionWindow> ();
					this.optionWindow = go.GetComponent<OptionWindow> ();
				}
				return "No More Purple";
			}
        }

        public string Description 
        {
            get { return "Purple is out!"; }
        }
		
		public static void disable ()
		{
			//TODO
		}
		
		public override void OnCreated (ILoading loading)
		{
			base.OnCreated (loading);
			if (optionWindow != null)
				this.optionWindow.Load ();
		}

		/*
		* When the level is loaded replace the existing value with the new one we assigned
		*/
		public override void OnLevelLoaded (LoadMode mode)
		{
			/* The value m_GrassPollutionColorOffset is accessed through a getter titled _GrassPollutionColorOffset */
			Shader.SetGlobalVector ("_GrassPollutionColorOffset", this.optionWindow.field_color.selectedColor);
			/* The value m_WaterColorDirty is accessed through a getter titled _WaterColorDirty */
			Shader.SetGlobalColor ("_WaterColorDirty", this.optionWindow.field_color.selectedColor);
		}
	}
}
