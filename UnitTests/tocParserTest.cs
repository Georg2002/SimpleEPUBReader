using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPUBParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class toc
	{
		[TestMethod]
		public void tocTest1()
		{
			var TestCase = new tocTestCase()
			{
				FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpubExtracted1, "OPS", "toc.ncx"),
				Chapters = 15,
				FirstChapterName = "とある魔術の禁書目録４",
				Title = "とある魔術の禁書目録４"
			};
			tocTester(TestCase);
		}

		[TestMethod]
		public void tocTest2()
		{
			var TestCase = new tocTestCase()
			{
				FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpubExtracted2, "toc.ncx"),
				Chapters = 14,
				FirstChapterName = "星界の断章 Ⅰ",
				Title = "[森岡浩之] 星界の断章1"
			};
			tocTester(TestCase);
		}

		private void tocTester(tocTestCase testCase)
		{
			var TextFile = new TextFile(File.ReadAllBytes(testCase.FilePath));
			TocInfo toc = new TocInfo(TextFile);
			Assert.IsTrue(toc.Title == testCase.Title);
			Assert.IsTrue(toc.Chapters.Count == testCase.Chapters);
			Assert.IsTrue(toc.Chapters[0] == testCase.FirstChapterName);
		}

		private class tocTestCase
		{
			public string FilePath;
			public int Chapters;
			public string FirstChapterName;
			public string Title;
		}
	}
}
