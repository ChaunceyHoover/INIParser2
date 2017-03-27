using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser {
	// Used to store key=value pair
	// Used for internal use; it's best to let the library use this instead of manually creating
	public struct INIElement
	{
		public string Key, Value;
	}

	// Note: These structs are meant for internal use; this helps maintain the 1-to-1 ratio for key to value
	public class INIConfig
	{
		public List<INIElement> Elements;
        public string FilePath;

		// Must specify config file on instanciation
		public INIConfig(string path)
		{
			FilePath = path;
			Elements = new List<INIElement>();

			// Open file
			List<string> fileIn = new List<string>();
			try
			{
				using (StreamReader sr = File.OpenText(path))
				{
					string temp;
					while ((temp = sr.ReadLine()) != null)
					{
						fileIn.Add(temp);
					}
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Unable to read file: " + e.Message);
			}

			// Process file content (fills Elements list with categories and elements)
			string[] lines = fileIn.ToArray();
			foreach (String line in lines)
			{
				if (line.Equals("") || line[0] == '#' || line[0] == ';')
					// Commented line
					continue;
				if (line[0] == '[')
				{
					// Start of a category
					INIElement element;
					element.Key = element.Value = line;
					Elements.Add(element);
				}
				else
				{
					// key=value
					char[] separator = { '=' };
					string[] processedLine = line.Split(separator, 2);
					if (processedLine[0].Equals(line))
						// Error in format; ignoring
						continue;
					INIElement element;
					element.Key = processedLine[0];
					element.Value = processedLine[1];
					Elements.Add(element);
				}
			}
		}

		// Gets the specified key from an INI file
		public INIElement GetValue(string category, string key)
		{
			for (int i = 0; i < Elements.Count; i++)
			{
				INIElement element = Elements[i];
				if (element.Key[0] == '[' && element.Key.Substring(1, element.Key.Length - 2).Equals(category))
				{
					// found category, searching for element
					for (int k = i + 1; k < Elements.Count; k++)
					{
						INIElement temp = Elements[k];
						if (temp.Key[0] == '[')
						{
							// reached next category, didn't find key
							INIElement blankElement = new INIElement();
							blankElement.Key = null;
							blankElement.Value = null;
							return blankElement;
						}
						if (temp.Key.Equals(key))
							return temp;
					}
				}
			}

			INIElement nullElement = new INIElement();
			nullElement.Key = null;
			nullElement.Value = null;
			return nullElement;
		}

		// Returns an array of every 
		public INIElement[] GetValues(string category)
		{
			List<INIElement> values = new List<INIElement>();
			bool foundCategory = false;

			foreach (INIElement element in Elements)
			{
				if (!foundCategory)
				{
					if (element.Key.Substring(1, element.Key.Length - 2).Equals(category))
						// found category, switching modes
						foundCategory = true;
				}
				else
				{
					if (element.Key.Substring(1, element.Key.Length - 2).Equals(category))
						// found next category, stopping loop
						break;
					values.Add(element);
				}
			}
			return values.ToArray();
		}

		// Changes a value of a key under the specified category
		public bool SetValue(string category, string key, string value)
		{
			if (key[0] == '[')
			{
				Console.Error.WriteLine("[INIConfig] Cannot change category name");
				return false;
			}

			for (int i = 0; i < Elements.Count; i++)
			{
				if (Elements[i].Key.Substring(1, Elements[i].Key.Length - 2).Equals(category))
				{
					for (int k = i + 1; k < Elements.Count; k++)
					{
						if (Elements[k].Key.Equals(key))
						{
							// apparently, just a simple `Elements[k].Value = value` is too good for C#
							INIElement copy = new INIElement();
							copy.Key = Elements[k].Key; copy.Value = value;
							Elements.Remove(Elements[k]);
							Elements.Insert(k, copy);
							return true;
						}
					}
				}
			}
			return false;
		}

		// Creates a new category if the specified category doesn't exist
		// note: do not put square brackets in parameter
		public bool CreateCategory(string category)
		{
			bool categoryExists = false;
			foreach (INIElement element in Elements)
			{
				if (element.Key[0] == '[' && element.Key.Substring(1, element.Key.Length - 2).Equals(category))
				{
					categoryExists = true;
					break;
				}
			}
			INIElement newCategory = new INIElement();
			newCategory.Key = newCategory.Value = "[" + category + "]";

			return !categoryExists;
		}

		// Saves file to the given path when class was instantiated
		public bool SaveFile()
		{
			return SaveFile(FilePath);
		}

		// Saves file to a new location and sets new save path to given path
		public bool SaveFile(string newPath)
		{
			FilePath = newPath;
			if (Elements.Count == 0)
			{
				Console.WriteLine("[INIConfig] Nothing to write to file (did you load the wrong file?)");
				return false;
			}
			try
			{
				FileStream fs = new FileStream(FilePath, FileMode.Create);
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.WriteLine(Elements[0].Key);
					for (int i = 1; i < Elements.Count; i++)
					{
						if (Elements[i].Key[0] == '[')
						{
							// Reached next category, adding line to separate categories
							sw.WriteLine();
							sw.WriteLine(Elements[i].Key);
						}
						else
							sw.WriteLine(Elements[i].Key + "=" + Elements[i].Value);
					}
				}
			} catch(Exception e)
			{
				Console.Error.WriteLine("Unable to save file at location: " + newPath);
				Console.Error.WriteLine("(try saving to a different location?)");
			}
			return false;
		}
	};
}
