using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Num = System.Numerics;

namespace SimpleCAD.Source.Utils
{
	// Courtesy of https://gist.github.com/prime31/91d1582624eb2635395417393018016e
	public class FilePicker
	{
		static readonly Dictionary<object, FilePicker> _filePickers = new Dictionary<object, FilePicker>();

		public string RootFolder;
		public string CurrentFolder;
		public string SelectedFile;
		public List<string> AllowedExtensions;
		public bool OnlyAllowFolders;
		public bool AllowCreation;

		private byte[] name = new byte[64];

		public static FilePicker GetFolderPicker(object o, string startingPath)
			=> GetFilePicker(o, startingPath, null, true);

		public static FilePicker GetFilePicker(object o, string startingPath, string searchFilter = null, bool onlyAllowFolders = false, bool allowCreate = false)
		{
			if (!Directory.Exists(startingPath))
			{
				startingPath = AppContext.BaseDirectory;
			}

			if (!_filePickers.TryGetValue(o, out FilePicker fp))
			{
				fp = new FilePicker();
				fp.RootFolder = startingPath;
				fp.CurrentFolder = startingPath;
				fp.OnlyAllowFolders = onlyAllowFolders;
				fp.AllowCreation = allowCreate;

				if (searchFilter != null)
				{
					if (fp.AllowedExtensions != null)
						fp.AllowedExtensions.Clear();
					else
						fp.AllowedExtensions = new List<string>();

					fp.AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
				}

				_filePickers.Add(o, fp);
			}

			return fp;
		}

		public static void RemoveFilePicker(object o) => _filePickers.Remove(o);

		public bool Draw()
		{
			ImGui.Text("Current Folder: " + Path.GetFileName(RootFolder) + CurrentFolder.Replace(RootFolder, ""));
			bool result = false;

			if (ImGui.BeginChildFrame(1, new Num.Vector2(400, 400)))
			{
				var di = new DirectoryInfo(CurrentFolder);
				if (di.Exists)
				{
					if (di.Parent != null && CurrentFolder != RootFolder)
					{
						if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
							CurrentFolder = di.Parent.FullName;
					}

					var fileSystemEntries = GetFileSystemEntries(di.FullName);
					foreach (var fse in fileSystemEntries)
					{
						if (Directory.Exists(fse))
						{
							var name = Path.GetFileName(fse);
							if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
								CurrentFolder = fse;
						}
						else
						{
							var name = Path.GetFileName(fse);
							bool isSelected = SelectedFile == fse;
							if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups))
								SelectedFile = fse;

							if (ImGui.IsMouseDoubleClicked(0))
							{
								result = true;
								ImGui.CloseCurrentPopup();
							}
						}
					}
				}
			}
			ImGui.EndChildFrame();

			if (AllowCreation)
			{
				ImGui.InputText("##File name", name, 64, ImGuiInputTextFlags.CharsNoBlank);
				ImGui.SameLine();
				if (ImGui.Button("Create"))
				{
					var fs = File.Create(CurrentFolder + "/" + Encoding.ASCII.GetString(name).Trim('\0'));
					fs.Close();
				}
			}

			if (ImGui.Button("Cancel"))
			{
				result = false;
				ImGui.CloseCurrentPopup();
			}

			if (OnlyAllowFolders)
			{
				ImGui.SameLine();
				if (ImGui.Button("Select"))
				{
					result = true;
					SelectedFile = CurrentFolder;
					ImGui.CloseCurrentPopup();
				}
			}
			else if (SelectedFile != null)
			{
				ImGui.SameLine();
				if (ImGui.Button("Select"))
				{
					result = true;
					ImGui.CloseCurrentPopup();
				}
			}

			return result;
		}

		List<string> GetFileSystemEntries(string fullName)
		{
			var files = new List<string>();
			var dirs = new List<string>();

			try
            {
				foreach (var fse in Directory.GetFileSystemEntries(fullName, ""))
				{
					if (Directory.Exists(fse))
					{
						dirs.Add(fse);
					}
					else if (!OnlyAllowFolders)
					{
						if (AllowedExtensions != null)
						{
							var ext = Path.GetExtension(fse);
							if (AllowedExtensions.Contains(ext))
								files.Add(fse);
						}
						else
						{
							files.Add(fse);
						}
					}
				}
			}
			catch (UnauthorizedAccessException e)
            {
				Console.WriteLine("Access denied to folder " + fullName);
            }
			

			var ret = new List<string>(dirs);
			ret.AddRange(files);

			return ret;
		}

	}
}