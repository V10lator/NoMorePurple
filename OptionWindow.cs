using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.DataBinding;
using System.Xml.Serialization;

namespace NoMorePurple
{

	public class OptionWindow : MonoBehaviour
	{
		private GameObject button_options;
		private GameObject panel;
		public UIColorField field_color;
		private bool ready = false;

		public void Start ()
		{
			//TODO: There must be a way to get the GameObject from NoMorePurple.instance instead of this slow hack
			GameObject modsList = GameObject.Find ("ModsList");
			if (modsList == null)
				return;
			GameObject mod = null;
			foreach (var item in modsList.GetComponentsInChildren<UILabel>().Where(l => l.name == "Name")) {
				if (item.text.Contains ("No More Purple")) {
					mod = item.transform.parent.gameObject;
					break;
				}
			}
			if (mod == null)
				return;
			
			
			UIButton button_share = mod.gameObject.transform.FindChild ("Share").GetComponent<UIButton> ();
			this.button_options = GameObject.Instantiate<GameObject> (button_share.gameObject);
			this.button_options.transform.SetParent (button_share.transform.parent);
			this.button_options.transform.localPosition = button_share.transform.localPosition;
			
			UIButton button_options = this.button_options.GetComponent<UIButton> ();
			button_options.isVisible = true;
			button_options.text = "Options";
			button_options.eventClick += Open;
			button_options.position += Vector3.left * button_share.width * 2.1f;
			
			GameObject panel_options = GameObject.Find ("(Library) OptionsPanel");
			this.panel = GameObject.Instantiate<GameObject> (panel_options);
			//TODO: We want to get rid of modsList
			this.panel.transform.SetParent (modsList.transform.parent.transform);
			GameObject.Destroy (this.panel.GetComponent<OptionsPanel> ());
			
			GameObject template = this.panel.GetComponentsInChildren<UICheckBox> ().Where (c => c.name == "EdgeScrolling").FirstOrDefault ().gameObject;
			GameObject.Destroy (template.GetComponent<BindProperty> ());
			Vector3 position = template.transform.localPosition;
			
			GameObject caption = null;
			foreach (Transform transform in this.panel.transform)
				if (transform.name == "Caption")
					caption = transform.gameObject;
				else
					GameObject.Destroy (transform.gameObject);
			
			UIPanel tmpPanel = this.panel.GetComponent<UIPanel> ();
			tmpPanel.autoFitChildrenVertically = true;
			tmpPanel.autoFitChildrenHorizontally = true;
			
			UIButton button_close = caption.transform.FindChild ("Close").GetComponent<UIButton> ();
			GameObject.Destroy (button_close.GetComponent<BindEvent> ());
			button_close.eventClick += Close;
			caption.transform.FindChild ("Label").GetComponent<UILabel> ().text = "No More Purple Color | Options";
			
			//TODO: We want to get rid of modsList
			GameObject oList = GameObject.Instantiate<GameObject> (modsList);
			for (int i = oList.transform.childCount - 1; i > -1; i--)
				Destroy (oList.transform.GetChild (i).gameObject);
			oList.transform.SetParent (this.panel.transform);
			oList.GetComponent<UIScrollablePanel> ().AlignTo (this.panel.GetComponent<UIPanel> (), UIAlignAnchor.TopLeft);
			oList.GetComponent<UIScrollablePanel> ().position += new Vector3 (
				caption.transform.FindChild ("Label").GetComponent<UILabel> ().height,
				-caption.transform.FindChild ("Label").GetComponent<UILabel> ().height * 2f
			);
			
			
			GameObject go = new GameObject ("No More Purples ColorField", typeof(UIColorField));
			this.field_color = go.GetComponent<UIColorField> ();
			this.field_color.transform.SetParent (oList.transform);
			this.field_color.isVisible = true;
//			this.field_color.zOrder = 0;
			
			GameObject save = GameObject.Instantiate<GameObject> (this.button_options);
			save.transform.SetParent (this.panel.transform);
			
			UIButton saveButton = save.GetComponent<UIButton> ();
			saveButton.isVisible = true;
			saveButton.eventClick += Save;
			saveButton.color = Color.green;
			saveButton.text = "OK";
			saveButton.AlignTo (panel.GetComponent<UIPanel> (), UIAlignAnchor.BottomRight);
			
			this.ready = true;
		}

		public void Load ()
		{
			string input;
			try {
				XmlSerializer serializer = new XmlSerializer (typeof(string));
				using (StreamReader reader = new StreamReader("NoMorePurple.xml")) {
					input = (string)serializer.Deserialize (reader);
					reader.Close ();
				}
			} catch (FileNotFoundException) {
				// No options file yet
				return;
			} catch (Exception e) {
				Debug.Log ("NoMorePurple: " + e.GetType ().Name + "while reading xml file: " + e.Message + "\n" + e.StackTrace);
				NoMorePurple.instance.disable ();
				return;
			}
			
			// Convert String to Color, see http://orbcreation.com/orbcreation/page.orb?974
			
			if (input == null || input.Length != 7) {
				Debug.Log ("NoMorePurple: Error reading xml file: Wanted Color, got \"" + input + "\"");
				NoMorePurple.instance.disable ();
				return;
			}
			
			Color color = new Color (0, 0, 0);
			try {
				string sub = input.Substring (1, input.Length - 1);
				color.r = (float)System.Int32.Parse (sub.Substring (0, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
				color.g = (float)System.Int32.Parse (sub.Substring (2, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
				color.b = (float)System.Int32.Parse (sub.Substring (4, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
			} catch (Exception e) {
				Debug.Log ("NoMorePurple: " + e.GetType ().Name + " while parsing xml file: " + e.Message + "\n" + e.StackTrace);
				NoMorePurple.instance.disable();
				return;
			}
			
			this.field_color.selectedColor = color;
		}
		
		void OnLevelWasLoaded(int level)
		{
			if (level == 5)
				this.ready = false;
		}
		
		void Update ()
		{
			if (!this.ready && Application.loadedLevel == 5) {
				try {
					Start ();
				} catch {
				}
			}
		}
		
		private void Open (UIComponent component, UIMouseEventParameter eventParam)
		{
			UIPanel panel = this.panel.GetComponent<UIPanel> ();
			panel.isVisible = true;
			panel.BringToFront ();
		}
		
		private void Close (UIComponent component, UIMouseEventParameter eventParam)
		{
			this.panel.GetComponent<UIPanel> ().isVisible = false;
		}
		
		private void Save (UIComponent component, UIMouseEventParameter eventParam)
		{
			Close (null, null);
			
			// Convert Color to String, see http://orbcreation.com/orbcreation/page.orb?974
			string hex = "#" + 
				this.field_color.selectedColor.r.ToString ("X2") + 
				this.field_color.selectedColor.g.ToString ("X2") + 
				this.field_color.selectedColor.b.ToString ("X2");
			
			try {
				XmlSerializer serializer = new XmlSerializer (typeof(string));
				using(StreamWriter writer = new StreamWriter("NoMorePurple.xml"))
				{
					serializer.Serialize(writer, hex);
					writer.Flush();
					writer.Close();
				}
			}
			catch(Exception e)
			{
				Debug.Log("NoMorePurple: "+e.GetType().Name+" while writing xml file: "+e.Message+"\n"+e.StackTrace);
			}
		}
		
		void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}
}
