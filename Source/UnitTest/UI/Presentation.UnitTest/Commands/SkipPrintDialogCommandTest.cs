﻿using NSubstitute;
using NUnit.Framework;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Query;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using pdfforge.PDFCreator.Core.Workflow.Exceptions;
using pdfforge.PDFCreator.Core.Workflow.Queries;
using pdfforge.PDFCreator.UI.Presentation.Commands;
using SystemWrapper.IO;

namespace Presentation.UnitTest.UserControls
{
    [TestFixture]
    public class SkipPrintDialogCommandTest
    {
        private ConversionProfile _pdfProfile;
        private IFileNameQuery _saveFileQuery;

        private static string _filepathFromSaveDialog = @"DirectoryFromSaveDialog\FilenameFromSaveDialog.pdf";

        [SetUp]
        public void Setup()
        {
            _pdfProfile = new ConversionProfile
            {
                Name = "PDF Profile",
                OutputFormat = OutputFormat.Pdf,
                FileNameTemplate = "X:\\test.pdf"
            };

            _saveFileQuery = Substitute.For<IFileNameQuery>();

            var outputFilenameResult = new OutputFilenameResult(_filepathFromSaveDialog, OutputFormat.Jpeg);
            var queryResultOutputFilenameResult = new QueryResult<OutputFilenameResult>(true, outputFilenameResult);

            _saveFileQuery.GetFileName(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OutputFormat>())
                .Returns(queryResultOutputFilenameResult);
        }

        private SkipPrintDialogCommand BuildCommand()
        {
            return new SkipPrintDialogCommand(_saveFileQuery);
        }

        private Job BuildJob(ConversionProfile profile)
        {
            var job = new Job(new JobInfo(), profile, new JobTranslations(), new Accounts());
            job.OutputFilenameTemplate = profile.FileNameTemplate;
            return job;
        }

        [Test]
        public void ShowSaveFileDialog_SetValuesFromUserInput_SetFilepathAndOutputFormatInJob()
        {
            var pathSafe = new PathWrapSafe();
            var skipPrintDialogCommand = BuildCommand();
            var job = BuildJob(_pdfProfile);
            skipPrintDialogCommand.Execute(job);

            var result = _saveFileQuery
                .GetFileName(pathSafe.GetDirectoryName(job.OutputFilenameTemplate),
                    pathSafe.GetFileName(job.OutputFilenameTemplate), job.Profile.OutputFormat);

            Assert.AreEqual(result.Data.Filepath, job.OutputFilenameTemplate);
            Assert.AreEqual(result.Data.OutputFormat, job.Profile.OutputFormat);
        }

        [Test]
        public void ShowSaveFileDialog_SetValuesFromUserInput_FilepathAndOutputFormatHasChanged()
        {
            var pathSafe = new PathWrapSafe();
            var skipPrintDialogCommand = BuildCommand();
            var job = BuildJob(_pdfProfile);

            var diffFilenameTemplate = job.OutputFilenameTemplate;
            var diffOutputFormat = job.Profile.OutputFormat;

            skipPrintDialogCommand.Execute(job);

            _saveFileQuery
                .GetFileName(pathSafe.GetDirectoryName(job.OutputFilenameTemplate),
                    pathSafe.GetFileName(job.OutputFilenameTemplate), job.Profile.OutputFormat);

            Assert.AreNotEqual(job.OutputFilenameTemplate, diffFilenameTemplate);
            Assert.AreNotEqual(job.Profile.OutputFormat, diffOutputFormat);
        }

        [Test]
        public void ShowSaveFileDialog_UserCancels_ThrowAbortWorkflowException()
        {
            var skipPrintDialogCommand = BuildCommand();
            var job = BuildJob(_pdfProfile);

            var userCanceledResult = new QueryResult<OutputFilenameResult> { Success = false };

            _saveFileQuery.GetFileName(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OutputFormat>()).Returns(userCanceledResult);

            Assert.Throws<AbortWorkflowException>(() => skipPrintDialogCommand.Execute(job));
        }
    }
}
