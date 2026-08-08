using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFileBrowser
{
	[Serializable]
	public struct FiletypeIcon
	{
		public string extension;
		public Sprite icon;
	}

	[CreateAssetMenu( fileName = "UI Skin", menuName = "yasirkula/SimpleFileBrowser/UI Skin", order = 111 )]
	public class UISkin : ScriptableObject
	{
		private int m_version = 0;
		public int Version { get { return m_version; } }

		[ContextMenu( "Refresh UI" )]
		private void Invalidate()
		{
			m_version = UnityEngine.Random.Range( int.MinValue / 2, int.MaxValue / 2 );
			initializedFiletypeIcons = false;
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			// Refresh all UIs that use this skin
			Invalidate();
		}
#endif

		[Header( "General" )]
		[SerializeField]
		private TMP_FontAsset m_font;
		public TMP_FontAsset Font
		{
			get { return m_font; }
			set { if( m_font != value ) { m_font = value; m_version++; } }
		}

		[SerializeField]
		private int m_fontSize = 14;
		public int FontSize
		{
			get { return m_fontSize; }
			set { if( m_fontSize != value ) { m_fontSize = value; m_version++; } }
		}

		[SerializeField]
		private float m_rowSpacing = 8f;
		public float RowSpacing
		{
			get { return m_rowSpacing; }
			set { if( m_rowSpacing != value ) { m_rowSpacing = value; m_version++; } }
		}

		[Header( "File Browser Window" )]

		[SerializeField]
		private Color m_filesListColor = Color.white;
		public Color FilesListColor
		{
			get { return m_filesListColor; }
			set { if( m_filesListColor != value ) { m_filesListColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_filesVerticalSeparatorColor = Color.grey;
		public Color FilesVerticalSeparatorColor
		{
			get { return m_filesVerticalSeparatorColor; }
			set { if( m_filesVerticalSeparatorColor != value ) { m_filesVerticalSeparatorColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_windowResizeGizmoColor = Color.black;
		public Color WindowResizeGizmoColor
		{
			get { return m_windowResizeGizmoColor; }
			set { if( m_windowResizeGizmoColor != value ) { m_windowResizeGizmoColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_headerButtonsColor = Color.white;
		public Color HeaderButtonsColor
		{
			get { return m_headerButtonsColor; }
			set { if( m_headerButtonsColor != value ) { m_headerButtonsColor = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_windowResizeGizmo;
		public Sprite WindowResizeGizmo
		{
			get { return m_windowResizeGizmo; }
			set { if( m_windowResizeGizmo != value ) { m_windowResizeGizmo = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_headerBackButton;
		public Sprite HeaderBackButton
		{
			get { return m_headerBackButton; }
			set { if( m_headerBackButton != value ) { m_headerBackButton = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_headerForwardButton;
		public Sprite HeaderForwardButton
		{
			get { return m_headerForwardButton; }
			set { if( m_headerForwardButton != value ) { m_headerForwardButton = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_headerUpButton;
		public Sprite HeaderUpButton
		{
			get { return m_headerUpButton; }
			set { if( m_headerUpButton != value ) { m_headerUpButton = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_headerContextMenuButton;
		public Sprite HeaderContextMenuButton
		{
			get { return m_headerContextMenuButton; }
			set { if( m_headerContextMenuButton != value ) { m_headerContextMenuButton = value; m_version++; } }
		}

		[Header( "Scrollbars" )]
		[SerializeField]
		private Color m_scrollbarBackgroundColor = Color.grey;
		public Color ScrollbarBackgroundColor
		{
			get { return m_scrollbarBackgroundColor; }
			set { if( m_scrollbarBackgroundColor != value ) { m_scrollbarBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_scrollbarColor = Color.black;
		public Color ScrollbarColor
		{
			get { return m_scrollbarColor; }
			set { if( m_scrollbarColor != value ) { m_scrollbarColor = value; m_version++; } }
		}

		[Header( "Files" )]
		[SerializeField]
		private float m_fileHeight = 30f;
		public float FileHeight
		{
			get { return m_fileHeight; }
			set { if( m_fileHeight != value ) { m_fileHeight = value; m_version++; } }
		}

		[SerializeField]
		private float m_fileIconsPadding = 6f;
		public float FileIconsPadding
		{
			get { return m_fileIconsPadding; }
			set { if( m_fileIconsPadding != value ) { m_fileIconsPadding = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileNormalBackgroundColor = Color.clear;
		public Color FileNormalBackgroundColor
		{
			get { return m_fileNormalBackgroundColor; }
			set { if( m_fileNormalBackgroundColor != value ) { m_fileNormalBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileAlternatingBackgroundColor = Color.clear;
		public Color FileAlternatingBackgroundColor
		{
			get { return m_fileAlternatingBackgroundColor; }
			set { if( m_fileAlternatingBackgroundColor != value ) { m_fileAlternatingBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileHoveredBackgroundColor = Color.cyan;
		public Color FileHoveredBackgroundColor
		{
			get { return m_fileHoveredBackgroundColor; }
			set { if( m_fileHoveredBackgroundColor != value ) { m_fileHoveredBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileSelectedBackgroundColor = Color.blue;
		public Color FileSelectedBackgroundColor
		{
			get { return m_fileSelectedBackgroundColor; }
			set { if( m_fileSelectedBackgroundColor != value ) { m_fileSelectedBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileNormalTextColor = Color.black;
		public Color FileNormalTextColor
		{
			get { return m_fileNormalTextColor; }
			set { if( m_fileNormalTextColor != value ) { m_fileNormalTextColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_fileSelectedTextColor = Color.black;
		public Color FileSelectedTextColor
		{
			get { return m_fileSelectedTextColor; }
			set { if( m_fileSelectedTextColor != value ) { m_fileSelectedTextColor = value; m_version++; } }
		}

		[Header( "File Icons" )]
		[SerializeField]
		private Sprite m_folderIcon;
		public Sprite FolderIcon
		{
			get { return m_folderIcon; }
			set { if( m_folderIcon != value ) { m_folderIcon = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_driveIcon;
		public Sprite DriveIcon
		{
			get { return m_driveIcon; }
			set { if( m_driveIcon != value ) { m_driveIcon = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_defaultFileIcon;
		public Sprite DefaultFileIcon
		{
			get { return m_defaultFileIcon; }
			set { if( m_defaultFileIcon != value ) { m_defaultFileIcon = value; m_version++; } }
		}

		[SerializeField]
		private FiletypeIcon[] m_filetypeIcons;
		public FiletypeIcon[] FiletypeIcons
		{
			get { return m_filetypeIcons; }
			set
			{
				if( m_filetypeIcons != value )
				{
					m_filetypeIcons = value;
					initializedFiletypeIcons = false;
					m_version++;
				}
			}
		}

		[NonSerialized] // Never save this value during domain reload (it's sometimes saved even though it's private)
		private bool initializedFiletypeIcons = false;
		private Dictionary<string, Sprite> filetypeToIcon;

		[NonSerialized]
		private bool m_allIconExtensionsHaveSingleSuffix = true;
		public bool AllIconExtensionsHaveSingleSuffix
		{
			get
			{
				if( !initializedFiletypeIcons )
					InitializeFiletypeIcons();

				return m_allIconExtensionsHaveSingleSuffix;
			}
		}

		[SerializeField]
		private Sprite m_fileMultiSelectionToggleOffIcon;
		public Sprite FileMultiSelectionToggleOffIcon
		{
			get { return m_fileMultiSelectionToggleOffIcon; }
			set { if( m_fileMultiSelectionToggleOffIcon != value ) { m_fileMultiSelectionToggleOffIcon = value; m_version++; } }
		}

		[SerializeField]
		private Sprite m_fileMultiSelectionToggleOnIcon;
		public Sprite FileMultiSelectionToggleOnIcon
		{
			get { return m_fileMultiSelectionToggleOnIcon; }
			set { if( m_fileMultiSelectionToggleOnIcon != value ) { m_fileMultiSelectionToggleOnIcon = value; m_version++; } }
		}

		[Header( "Context Menu" )]
		[SerializeField]
		private Color m_contextMenuBackgroundColor = Color.grey;
		public Color ContextMenuBackgroundColor
		{
			get { return m_contextMenuBackgroundColor; }
			set { if( m_contextMenuBackgroundColor != value ) { m_contextMenuBackgroundColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_contextMenuTextColor = Color.black;
		public Color ContextMenuTextColor
		{
			get { return m_contextMenuTextColor; }
			set { if( m_contextMenuTextColor != value ) { m_contextMenuTextColor = value; m_version++; } }
		}

		[SerializeField]
		private Color m_contextMenuSeparatorColor = Color.black;
		public Color ContextMenuSeparatorColor
		{
			get { return m_contextMenuSeparatorColor; }
			set { if( m_contextMenuSeparatorColor != value ) { m_contextMenuSeparatorColor = value; m_version++; } }
		}

		[Header( "Popup Panels" )]
		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs( "m_deletePanelBackgroundColor" )]
		private Color m_popupPanelsBackgroundColor = Color.grey;
		public Color PopupPanelsBackgroundColor
		{
			get { return m_popupPanelsBackgroundColor; }
			set { if( m_popupPanelsBackgroundColor != value ) { m_popupPanelsBackgroundColor = value; m_version++; } }
		}

		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs( "m_deletePanelTextColor" )]
		private Color m_popupPanelsTextColor = Color.black;
		public Color PopupPanelsTextColor
		{
			get { return m_popupPanelsTextColor; }
			set { if( m_popupPanelsTextColor != value ) { m_popupPanelsTextColor = value; m_version++; } }
		}

		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs( "m_deletePanelBackground" )]
		private Sprite m_popupPanelsBackground;
		public Sprite PopupPanelsBackground
		{
			get { return m_popupPanelsBackground; }
			set { if( m_popupPanelsBackground != value ) { m_popupPanelsBackground = value; m_version++; } }
		}

		public void ApplyTo( TMP_Text text, Color textColor )
		{
			text.color = textColor;
			text.font = m_font;
			text.fontSize = m_fontSize;
		}

		public void ApplyTo( TMP_InputField inputField )
		{

		}

		public void ApplyTo( Button button )
		{

		}

		public void ApplyTo( Scrollbar scrollbar )
		{
			scrollbar.GetComponent<Image>().color = m_scrollbarBackgroundColor;
			scrollbar.image.color = m_scrollbarColor;
		}

		public Sprite GetIconForFileEntry( in FileSystemEntry fileInfo, bool extensionMayHaveMultipleSuffixes )
		{
			if( !initializedFiletypeIcons )
				InitializeFiletypeIcons();

			Sprite icon;
			if( fileInfo.IsDirectory )
				return m_folderIcon;
			else if( filetypeToIcon.TryGetValue( fileInfo.Extension, out icon ) )
				return icon;
			else if( extensionMayHaveMultipleSuffixes )
			{
				for( int i = 0; i < m_filetypeIcons.Length; i++ )
				{
					if( fileInfo.Extension.EndsWith( m_filetypeIcons[i].extension, StringComparison.Ordinal ) )
					{
						filetypeToIcon[fileInfo.Extension] = m_filetypeIcons[i].icon;
						return m_filetypeIcons[i].icon;
					}
				}
			}

			filetypeToIcon[fileInfo.Extension] = m_defaultFileIcon;
			return m_defaultFileIcon;
		}

		private void InitializeFiletypeIcons()
		{
			initializedFiletypeIcons = true;

			if( filetypeToIcon == null )
				filetypeToIcon = new Dictionary<string, Sprite>( 128 );
			else
				filetypeToIcon.Clear();

			m_allIconExtensionsHaveSingleSuffix = true;

			for( int i = 0; i < m_filetypeIcons.Length; i++ )
			{
				m_filetypeIcons[i].extension = m_filetypeIcons[i].extension.ToLowerInvariant();
				if( m_filetypeIcons[i].extension[0] != '.' )
					m_filetypeIcons[i].extension = "." + m_filetypeIcons[i].extension;

				filetypeToIcon[m_filetypeIcons[i].extension] = m_filetypeIcons[i].icon;

				m_allIconExtensionsHaveSingleSuffix &= ( m_filetypeIcons[i].extension.LastIndexOf( '.' ) == 0 );
			}
		}
	}
}