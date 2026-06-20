using NUnit.Framework;
using WebDocTruyen.Infrastructure.Repositories;

namespace WebDocTruyen.Tests
{
    [TestFixture]
    public class GenerateSlugTests
    {
        // ── Nhóm 1: Trường hợp cơ bản (TC01 - TC05) ──────────────
        [TestCase("hello world", "hello-world", TestName = "TC01_ChuoiThuongCoKhoangTrang")]
        [TestCase("HELLO WORLD", "hello-world", TestName = "TC02_ChuoiHoaChuyenThanhThuong")]
        [TestCase("hello-world", "hello-world", TestName = "TC03_SlugDaHopLeGiuNguyen")]
        [TestCase("A", "a", TestName = "TC04_MotKyTuDuyNhat")]
        [TestCase("already-valid-slug123", "already-valid-slug123", TestName = "TC05_SlugHopLeCoChuSo")]

        // ── Nhóm 2: Tiếng Việt có dấu (TC06 - TC09) ──────────────
        [TestCase("Tiên Nữ", "tien-nu", TestName = "TC06_TiengVietBoDauNguyenAm")]
        [TestCase("Nguyễn Văn A", "nguyen-van-a", TestName = "TC07_HoTenTiengVietDayDu")]
        [TestCase("Đại Chiến", "ai-chien", TestName = "TC08_KyTuDxoaHan_KhongThanhD")]
        [TestCase("Truyện: Đại Chiến!", "truyen-ai-chien", TestName = "TC09_TiengVietKemDauCauVaKyTuDacBiet")]

        // ── Nhóm 3: Khoảng trắng đặc biệt (TC10 - TC14) ──────────
        [TestCase("hello   world", "hello---world", TestName = "TC10_NhieuKhoangTrangLienTiep")]
        [TestCase(" hello world ", "-hello-world-", TestName = "TC11_KhoangTrangDauVaCuoiKhongBiCat")]
        [TestCase("hello\tworld", "helloworld", TestName = "TC12_KyTuTabBiXoaHoanToan")]
        [TestCase("   ", "", TestName = "TC13_ChuoiToanKhoangTrangTraVeRong")]
        [TestCase("", "", TestName = "TC14_ChuoiRongTraVeRong")]

        // ── Nhóm 4: Số / ký tự đặc biệt (TC15 - TC20) ────────────
        [TestCase("Chapter 123", "chapter-123", TestName = "TC15_ChuSoDuocGiuLai")]
        [TestCase("123456", "123456", TestName = "TC16_ChuoiChiToanSo")]
        [TestCase("hello_world", "helloworld", TestName = "TC17_GachDuoiBiXoa")]
        [TestCase("J.K. Rowling!!!", "jk-rowling", TestName = "TC18_DauCauVaNhieuDauChamThan")]
        [TestCase("50% Off!", "50-off", TestName = "TC19_KyTuPhanTramVaChamThan")]
        [TestCase("ASP.NET Core", "aspnet-core", TestName = "TC20_DauChamTrongTenRieng")]
        public void GenerateSlug_TraVeKetQuaDungMongDoi(string input, string expected)
        {
            var actual = StoryRepository.GenerateSlug(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GenerateSlug_DauVaoNull_TraVeChuoiRong()
        {
            var actual = StoryRepository.GenerateSlug(null!);
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [TestCase("Tiên Nữ Hạ Phàm")]
        [TestCase("Đại Chiến Tam Quốc")]
        [TestCase("Hello World 123")]
        public void GenerateSlug_TinhChatIdempotent_SlugCuaSlugBangChinhNo(string input)
        {
            var slug1 = StoryRepository.GenerateSlug(input);
            var slug2 = StoryRepository.GenerateSlug(slug1);
            Assert.That(slug2, Is.EqualTo(slug1));
        }

        [Test]
        public void GenerateSlug_KetQuaKhongChuaKyTuHoa()
        {
            var actual = StoryRepository.GenerateSlug("HeLLo WoRLD");
            Assert.That(actual, Is.EqualTo(actual.ToLower()));
        }

        [Test]
        public void GenerateSlug_KetQuaKhongChuaKhoangTrang()
        {
            var actual = StoryRepository.GenerateSlug("Truyện Tranh Hay Nhất 2026");
            Assert.That(actual, Does.Not.Contain(" "));
        }
    }
}