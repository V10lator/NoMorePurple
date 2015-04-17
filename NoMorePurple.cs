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

    public class NoMorePurple : MonoBehaviour, IUserMod
    {
		public OptionWindow optionWindow = null;
		private static NoMorePurple myself = null;

		public NoMorePurple ()
		{
			NoMorePurple.myself = this;
		}
		
        public string Name {
			get {
				if (this.optionWindow == null) {
					GameObject go = new GameObject ("No More Purples OptionWindow");
					go.AddComponent<OptionWindow> ();
					this.optionWindow = go.GetComponent<OptionWindow> ();
					this.optionWindow.Load ();
				}
				return "No More Purple";
			}
        }

        public string Description 
        {
            get { return "Define your own color for waste!"; }
        }
		
		public static NoMorePurple instance
		{
			get { return NoMorePurple.myself; }
		}
		
		public void disable ()
		{
			//TODO
			Debug.Log ("---- Disable called!");
		}
		
		void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}
	
	public class NMPListener : LoadingExtensionBase
	{
		public override void OnCreated (ILoading loading)
		{
			base.OnCreated (loading);
			if (NoMorePurple.instance.optionWindow != null && !NoMorePurple.instance.optionWindow.ready) {
				NoMorePurple.instance.optionWindow.Start ();
			}
		}

		/*
		* When the level is loaded replace the existing value with the new one we assigned
		*/
		public override void OnLevelLoaded (LoadMode mode)
		{
			/* The value m_GrassPollutionColorOffset is accessed through a getter titled _GrassPollutionColorOffset */
			Shader.SetGlobalColor ("_GrassPollutionColorOffset", NoMorePurple.instance.optionWindow.color);
			/* The value m_WaterColorDirty is accessed through a getter titled _WaterColorDirty */
			Shader.SetGlobalColor ("_WaterColorDirty", NoMorePurple.instance.optionWindow.color);
		}
	}
}
