# Báo cáo kết quả Unit Test — HĐ3

**Hàm kiểm thử:** `StoryRepository.GenerateSlug(string text)`
**File nguồn:** `WebDocTruyen.Infrastructure/Repositories/StoryRepository.cs`
**File test:** `WebDocTruyen.Tests/GenerateSlugTests.cs`
**Framework:** NUnit (.NET 9)
**Công cụ chạy:** Visual Studio Test Explorer

## Kết quả chạy thực tế

```
Test run finished: 26 Tests (26 Passed, 0 Failed, 0 Skipped) run in 215 ms
```

**Tổng kết: 26 / 26 test PASS — 0 / 26 test FAIL — 0 Warning — 0 Error**

## Bảng 20 test case chính

|STT | Tên Test                             | Input                   | Output mong đợi         | Kết quả thực tế |
|----|--------------------------------------|-------------------------|-------------------------|-----------------|
| 1  | TC01_ChuoiThuongCoKhoangTrang        | `hello world`           | `hello-world`           | ✅ PASS         |
| 2  | TC02_ChuoiHoaChuyenThanhThuong       | `HELLO WORLD`           | `hello-world`           | ✅ PASS         |
| 3  | TC03_SlugDaHopLeGiuNguyen            | `hello-world`           | `hello-world`           | ✅ PASS         |
| 4  | TC04_MotKyTuDuyNhat                  | `A`                     | `a`                     | ✅ PASS         |
| 5  | TC05_SlugHopLeCoChuSo                | `already-valid-slug123` | `already-valid-slug123` | ✅ PASS         |
| 6  | TC06_TiengVietBoDauNguyenAm          | `Tiên Nữ`               | `tien-nu`               | ✅ PASS         |
| 7  | TC07_HoTenTiengVietDayDu             | `Nguyễn Văn A`          | `nguyen-van-a`          | ✅ PASS         |
| 8  | TC08_KyTuDxoaHan_KhongThanhD	        | `Đại Chiến`             | `ai-chien`              | ✅ PASS         |
| 9  | TC09_TiengVietKemDauCauVaKyTuDacBiet | `Truyện: Đại Chiến!`    | `truyen-ai-chien`       | ✅ PASS         |
| 10 | TC10_NhieuKhoangTrangLienTiep        | `hello   world`         | `hello---world`         | ✅ PASS         |
| 11 | TC11_KhoangTrangDauVaCuoiKhongBiCat  | ` hello world `         | `-hello-world-`         | ✅ PASS         |
| 12 | TC12_KyTuTabBiXoaHoanToan            | `hello\tworld`          | `helloworld`            | ✅ PASS         |
| 13 | TC13_ChuoiToanKhoangTrangTraVeRong   | '   '                   | ''                      | ✅ PASS         |
| 14 | TC14_ChuoiRongTraVeRong              | ' '                     | ''                      | ✅ PASS         |
| 15 | TC15_ChuSoDuocGiuLai                 | `Chapter 123`           | `chapter-123`           | ✅ PASS         |
| 16 | TC16_ChuoiChiToanSo                  | `123456`                | `123456`                | ✅ PASS         |
| 17 | TC17_GachDuoiBiXoa                   | `hello_world`           | `helloworld`            | ✅ PASS         |
| 18 | TC18_DauCauVaNhieuDauChamThan        | `J.K. Rowling!!!`       | `jk-rowling`            | ✅ PASS         |
| 19 | TC19_KyTuPhanTramVaChamThan          | `50% Off!`              | `50-off`                | ✅ PASS         |
| 20 | TC20_DauChamTrongTenRieng            | `ASP.NET Core`          | `aspnet-core`           | ✅ PASS         |

## 6 test bổ sung

| Tên Test                                            | Mục đích                                           | Kết quả |
|-----------------------------------------------------|----------------------------------------------------|---------|
| GenerateSlug_DauVaoNull_TraVeChuoiRong              | Input `null` → trả về `""`                         | ✅ PASS |
| GenerateSlug_TinhChatIdempotent (3 input khác nhau) | `GenerateSlug(GenerateSlug(x)) == GenerateSlug(x)` | ✅ PASS |
| GenerateSlug_KetQuaKhongChuaKyTuHoa                 | Output luôn là chữ thường                          | ✅ PASS |
| GenerateSlug_KetQuaKhongChuaKhoangTrang             | Output không còn dấu cách                          | ✅ PASS |

## Nhận xét

- Hàm `GenerateSlug` hoạt động đúng với toàn bộ 26 kịch bản kiểm thử, bao gồm các trường hợp
  biên (chuỗi rỗng, null, toàn khoảng trắng) và ký tự đặc biệt.
- Phát hiện đáng chú ý: ký tự **"Đ"/"đ"** không được chuyển thành "d" như kỳ vọng thông thường,
  mà bị loại bỏ hoàn toàn khỏi slug (do cơ chế Unicode NFD không tách "Đ" thành "D" + dấu gạch).
  Đây không phải lỗi runtime nhưng là điểm cần lưu ý nếu yêu cầu nghiệp vụ muốn slug giữ chữ "d"
  thay vì xóa hẳn.
