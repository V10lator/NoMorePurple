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
		public Color grassColor, waterColor;
		private Color? tmpGrassColor = null, tmpWaterColor = null;
		public bool ready = false;
		
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
			
			
			GameObject save = GameObject.Instantiate<GameObject> (this.button_options);
			save.transform.SetParent (this.panel.transform);
			UIButton saveButton = save.GetComponent<UIButton> ();
			saveButton.isVisible = true;
			saveButton.eventClick += Save;
			saveButton.color = Color.green;
			saveButton.text = "OK";
			saveButton.AlignTo (panel.GetComponent<UIPanel> (), UIAlignAnchor.BottomRight);
			
			GameObject go = new GameObject ("No More Purples ColorPicker", typeof(UILabel));
			UILabel label = go.GetComponent<UILabel> ();
			label.text = "Test";
			label.transform.SetParent (oList.transform);
			label.isVisible = true;
			label.zOrder = 0;
			/*
			go = new GameObject ("No More Purples ColorPicker", typeof(UIColorPicker));
			UIColorPicker field = go.GetComponent<UIColorPicker> ();
			Debug.Log ("----- field parent: " + field.transform);
			Debug.Log ("----- field.component parent: " + field.component.transform);
			field.component.transform.SetParent (oList.transform);
			field.transform.SetParent (field.component.transform);
			field.color = this.color;
			field.eventColorUpdated += updateTmpColor;
			field.component.isVisible = true;
			field.component.zOrder = 1;
			*/
			this.ready = true;
		}

		public void Load ()
		{
			XmlHolder holder = null;
			try {
				XmlSerializer serializer = new XmlSerializer (typeof(XmlHolder));
				using (StreamReader reader = new StreamReader("NoMorePurple.xml")) {
					holder = (XmlHolder)serializer.Deserialize (reader);
					reader.Close ();
					
				}
			} catch (FileNotFoundException) {
				// No options file yet
				this.grassColor = this.waterColor = new Color (0f, 0f, 0f);
				return;
			} catch (Exception e) {
				Debug.Log ("NoMorePurple: " + e.GetType ().Name + " while reading xml file: " + e.Message + "\n" + e.StackTrace + "\n\n");
				if (e.InnerException != null) 
					Debug.Log ("Caused by: " + e.InnerException.GetType ().Name + ": " + e.InnerException.Message + "\n" + e.InnerException.StackTrace);
				NoMorePurple.instance.disable ();
				return;
			}
			
			// Convert String to Color
			if (holder == null) {
				Debug.Log ("NoMorePurple: Error reading xml file: Wanted XmlHolder, got null");
				NoMorePurple.instance.disable ();
				return;
			}
			if (holder.grassColor == null || holder.grassColor.Length != 7) {
				Debug.Log ("NoMorePurple: Error reading xml file: Wanted Color, got \"" + holder.grassColor + "\"");
				NoMorePurple.instance.disable ();
				return;
			}
			if (holder.waterColor == null || holder.waterColor.Length != 7) {
				Debug.Log ("NoMorePurple: Error reading xml file: Wanted Color, got \"" + holder.waterColor + "\"");
				NoMorePurple.instance.disable ();
				return;
			}
			
			Color grassColor, waterColor;
			try {
				grassColor = toColor (holder.grassColor);
				waterColor = toColor (holder.waterColor);
			} catch (Exception e) {
				Debug.Log ("NoMorePurple: " + e.GetType ().Name + " while parsing xml file: " + e.Message + "\n" + e.StackTrace);
				NoMorePurple.instance.disable ();
				return;
			}
			
			this.grassColor = grassColor;
			this.waterColor = waterColor;
		}
		
		private Color toColor (string hex)
		{
			Color color = new Color (0f, 0f, 0f);
			try {
				color.r = (float)System.Int32.Parse (hex.Substring (1, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
				color.g = (float)System.Int32.Parse (hex.Substring (3, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
				color.b = (float)System.Int32.Parse (hex.Substring (5, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
			} catch (Exception e) {
				throw e;
			}
			return color;
		}
		
		void OnLevelWasLoaded(int level)
		{
			if (level == 5)
				this.ready = false;
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
			this.tmpGrassColor = this.tmpWaterColor = null;
		}
		
		private string toHex(Color32 aColor) {
			String rs = Convert.ToString(aColor.r,16);
			String gs = Convert.ToString(aColor.g,16);
			String bs = Convert.ToString(aColor.b,16);
			while(rs.Length < 2) rs= "0" + rs;
			while(gs.Length < 2) gs= "0" + gs;
			while(bs.Length < 2) bs= "0" + bs;
			return "#"+ rs + gs + bs;
		}
		
		private void Save (UIComponent component, UIMouseEventParameter eventParam)
		{
			if (this.tmpGrassColor != null)
				this.grassColor = this.tmpGrassColor.Value;
			if (this.tmpWaterColor != null)
				this.waterColor = this.tmpWaterColor.Value;
			Close (null, null);
			
			try {
				XmlSerializer serializer = new XmlSerializer (typeof(XmlHolder));
				using (StreamWriter writer = new StreamWriter("NoMorePurple.xml")) {
					serializer.Serialize (writer, new XmlHolder (toHex (this.grassColor), toHex (this.waterColor)));
					writer.Flush ();
					writer.Close ();
				}
			} catch (Exception e) {
				Debug.Log ("NoMorePurple: " + e.GetType ().Name + " while writing xml file: " + e.Message + "\n" + e.StackTrace);
				if (e.InnerException != null) 
					Debug.Log ("Caused by: " + e.InnerException.GetType ().Name + ": " + e.InnerException.Message + "\n" + e.InnerException.StackTrace);
			}
		}
		
		private void updateTmpColor (Color color, bool grass)
		{
			if (grass)
				this.tmpGrassColor = color;
			else
				this.tmpWaterColor = color;
		}
		
		void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}
	
	[XmlRoot("No_More_Purple_Configuration")]
	public class XmlHolder
	{
		[XmlElement(ElementName="File_version")]
		public double version = 0.1;
			
    	[XmlElement(ElementName="Grass_color")]
		public string grassColor;

    	[XmlElement("Water_color")]
		public string waterColor;
		
		// This is needed, else we get
		// InvalidOperationException: NoMorePurple.XmlHolder cannot be serialized because it does not have a default public constructor
		public XmlHolder ()
		{
			this.grassColor = this.waterColor = "[INVALID]";
		}
		
		public XmlHolder (string grassColor, string waterColor)
		{
			this.grassColor = grassColor;
			this.waterColor = waterColor;
		}
	}
}
