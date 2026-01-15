using System;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Domain.Tests.ValueObjects
{
    public class StreamingUrlTests
    {
        [Fact]
        public void Constructor_ShouldSetValuesCorrectly()
        {
            var url = "http://example.com/stream.m3u8";
            var sut = new StreamingUrl(url);
            
            Assert.Equal(url, sut.Value);
            Assert.False(sut.IsEncrypted);
        }

        [Fact]
        public void Constructor_WithEmptyUrl_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new StreamingUrl(""));
            Assert.Throws<ArgumentException>(() => new StreamingUrl(null));
            Assert.Throws<ArgumentException>(() => new StreamingUrl("   "));
        }

        [Fact]
        public void Create_ShouldReturnInstance()
        {
            var url = "http://test.com";
            var sut = StreamingUrl.Create(url);
            Assert.Equal(url, sut.Value);
        }

        [Fact]
        public void Encrypt_ShouldReturnNewEncryptedInstance()
        {
            var url = "original";
            var sut = new StreamingUrl(url);
            
            var encrypted = sut.Encrypt(s => "encrypted_" + s);
            
            Assert.NotSame(sut, encrypted);
            Assert.Equal("encrypted_original", encrypted.Value);
            Assert.True(encrypted.IsEncrypted);
            // Original should remain unchanged
            Assert.Equal("original", sut.Value);
            Assert.False(sut.IsEncrypted);
        }

        [Fact]
        public void Encrypt_WhenAlreadyEncrypted_ShouldReturnSameInstance()
        {
            var sut = new StreamingUrl("encoded", true);
            var result = sut.Encrypt(s => "double_" + s);

            Assert.Same(sut, result);
            Assert.Equal("encoded", result.Value);
        }

        [Fact]
        public void Decrypt_ShouldReturnDecryptedString()
        {
            var sut = new StreamingUrl("encrypted_data", true);
            var result = sut.Decrypt(s => s.Replace("encrypted_", ""));
            
            Assert.Equal("data", result);
        }

        [Fact]
        public void Decrypt_WhenNotEncrypted_ShouldReturnOriginalValue()
        {
            var sut = new StreamingUrl("plain");
            var result = sut.Decrypt(s => "wrong");
            
            Assert.Equal("plain", result);
        }
    }
}
