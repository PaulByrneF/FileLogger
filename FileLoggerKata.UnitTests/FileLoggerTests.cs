using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileLoggerKata.UnitTests
{

    [TestClass]
    public class FileLoggerTests
    {
        private static readonly DateTime Saturday = new DateTime(2022, 3, 12);
        private static readonly DateTime Sunday = new DateTime(2022, 3, 13);
        private const string Message = "test";
        private FileLogger FileLogger { get; }
        private Mock<IFileSystem> FileSysMock { get; }
        private Mock<IDateProvider> DateProvMock { get; }
        private DateTime DefaultToday => new DateTime(2022, 3, 14);
        private string DefaultLogFileName => $"log{DefaultToday:yyyyMMdd}.txt";

        public FileLoggerTests()
        {
            FileSysMock = new Mock<IFileSystem>(MockBehavior.Strict);
            FileSysMock.Setup(fs => fs.Append(It.IsNotNull<string>(), It.IsNotNull<string>()));
            FileSysMock.Setup(fs => fs.Create(It.IsNotNull<string>()));
            FileSysMock.Setup(fs => fs.Exists(It.IsNotNull<string>())).Returns(true);
            FileSysMock.Setup(fs => fs.GetLastWriteTime(It.IsNotNull<string>())).Returns(DateTime.Now);
            FileSysMock.Setup(fs => fs.Rename(It.IsNotNull<string>(), It.IsNotNull<string>()));

            DateProvMock = new Mock<IDateProvider>(MockBehavior.Strict);
            DateProvMock.Setup(dp => dp.Today).Returns(DefaultToday);

            FileLogger = new FileLogger(FileSysMock.Object, DateProvMock.Object);
        }

        [TestMethod]
        public void LogMessage_WeekDayFileExists_OnMonday()
        {
            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Exists(DefaultLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.Create(DefaultLogFileName), Times.Never);
            FileSysMock.Verify(fs => fs.Append(DefaultLogFileName, Message), Times.Once);
        }

        [TestMethod]
        public void LogMessage_WeekendFileExists_OnSaturday()
        {
            const string expectedFileName = "weekend.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Saturday);

            FileLogger.Log(Message);

            DateProvMock.VerifyGet(dp => dp.Today, Times.AtLeastOnce);
            FileSysMock.Verify(fs => fs.Append(expectedFileName, Message), Times.Once);
        }

        [TestMethod]
        public void LogMessage_WeekendFileExists_OnSunday()
        {
            const string expectedFileName = "weekend.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Sunday);

            FileLogger.Log(Message);

            DateProvMock.VerifyGet(dp => dp.Today, Times.AtLeastOnce);
            FileSysMock.Verify(fs => fs.Append(expectedFileName, Message), Times.Once);
        }

        [TestMethod]
        public void CreateFileAppendMessage_WeekDayNotExists_OnMonday()
        {
            FileSysMock.Setup(fs => fs.Exists(DefaultLogFileName)).Returns(false);

            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Exists(DefaultLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.Create(DefaultLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.Append(DefaultLogFileName, Message), Times.Once);
        }

        [TestMethod]
        public void DetermineDateLogMessage_OnMonday()
        {
            FileLogger.Log(Message);

            DateProvMock.VerifyGet(dp => dp.Today, Times.Exactly(2));
            FileSysMock.Verify(fs => fs.Append(DefaultLogFileName, Message), Times.Once);
        }

        [TestMethod]
        public void LogMessage_ArchivingOneWeekOldLogFile_OnSaturday()
        {
            var prevSunday = Saturday.AddDays(-6);
            var prevSundaySeconds = prevSunday.Add(new TimeSpan(23, 59, 59));
            var expectedFileName = "weekend.txt";
            var archiveLogFileName = $"weekend-{prevSunday:yyyyMMdd}.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Saturday);
            FileSysMock.Setup(fs => fs.GetLastWriteTime(expectedFileName)).Returns(prevSundaySeconds);

            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Rename(expectedFileName, archiveLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.GetLastWriteTime(expectedFileName), Times.AtLeastOnce);
        }

        [TestMethod]
        public void LogMessage_ArchivingOneWeekOldLogFile_OnSunday()
        {
            var prevSunday = Sunday.AddDays(-7);
            var prevSundaySeconds = prevSunday.Add(new TimeSpan(23, 59, 59));
            var expectedFileName = "weekend.txt";
            var archiveLogFileName = $"weekend-{prevSunday:yyyyMMdd}.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Sunday);
            FileSysMock.Setup(fs => fs.GetLastWriteTime(expectedFileName)).Returns(prevSundaySeconds);

            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Rename(expectedFileName, archiveLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.GetLastWriteTime(expectedFileName), Times.AtLeastOnce);
        }

        [TestMethod]
        public void LogMessage_ArchivingSevenWeekOldLogFile_OnSaturday()
        {
            var prevSunday = Saturday.AddDays(-48);
            var prevSundaySeconds = prevSunday.Add(new TimeSpan(23, 59, 59));
            var expectedFileName = "weekend.txt";
            var archiveLogFileName = $"weekend-{prevSunday:yyyyMMdd}.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Saturday);
            FileSysMock.Setup(fs => fs.GetLastWriteTime(expectedFileName)).Returns(prevSundaySeconds);

            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Rename(expectedFileName, archiveLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.GetLastWriteTime(expectedFileName), Times.AtLeastOnce);
        }

        [TestMethod]
        public void LogMessage_ArchivingSevenWeekOldLogFile_OnSunday()
        {
            var prevSunday = Sunday.AddDays(-49);
            var prevSundaySeconds = prevSunday.Add(new TimeSpan(23, 59, 59));
            var expectedFileName = "weekend.txt";
            var archiveLogFileName = $"weekend-{prevSunday:yyyyMMdd}.txt";
            DateProvMock.Setup(dp => dp.Today).Returns(Sunday);
            FileSysMock.Setup(fs => fs.GetLastWriteTime(expectedFileName)).Returns(prevSundaySeconds);

            FileLogger.Log(Message);

            FileSysMock.Verify(fs => fs.Rename(expectedFileName, archiveLogFileName), Times.Once);
            FileSysMock.Verify(fs => fs.GetLastWriteTime(expectedFileName), Times.AtLeastOnce);
        }
    }
}