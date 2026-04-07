using NUnit.Framework;
using Domain.Services;

namespace Tests.Domain.Services
{
    public class BaseHealthServiceTests
    {
        private BaseHealthService _baseHealthService;
        private const int StartingHealth = 100;

        [SetUp]
        public void Setup()
        {
            _baseHealthService = new BaseHealthService(StartingHealth);
        }

        [Test]
        public void Constructor_ShouldSetInitialHealthCorrectly()
        {
            Assert.AreEqual(StartingHealth, _baseHealthService.MaxHealth);
            Assert.AreEqual(StartingHealth, _baseHealthService.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ShouldReduceHealth_WhenAmountIsPositive()
        {
            _baseHealthService.TakeDamage(20);
            Assert.AreEqual(80, _baseHealthService.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ShouldNotReduceHealthBelowZero()
        {
            _baseHealthService.TakeDamage(StartingHealth + 50);
            Assert.AreEqual(0, _baseHealthService.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ShouldInvokeOnHealthChangedEvent_WithCorrectValue()
        {
            int invokedHealthValue = -1;
            int eventInvokeCount = 0;
            _baseHealthService.OnHealthChanged += (newHealth) => 
            {
                invokedHealthValue = newHealth;
                eventInvokeCount++;
            };

            _baseHealthService.TakeDamage(30);
            Assert.AreEqual(1, eventInvokeCount, "Sự kiện thay đổi máu phải được gọi chính xác 1 lần.");
            Assert.AreEqual(70, invokedHealthValue, "Giá trị truyền vào Event phải là lượng máu sau khi bị trừ.");
        }

        [Test]
        public void TakeDamage_ShouldInvokeOnBaseDestroyedEvent_WhenHealthReachesZero()
        {
            bool isDestroyedInvoked = false;
            _baseHealthService.OnBaseDestroyed += () => isDestroyedInvoked = true;
            _baseHealthService.TakeDamage(StartingHealth);
            Assert.IsTrue(isDestroyedInvoked, "Sự kiện OnBaseDestroyed phải được gọi khi máu về 0.");
        }

        [Test]
        public void TakeDamage_ShouldNotDoAnything_WhenBaseIsAlreadyDestroyed()
        {
            _baseHealthService.TakeDamage(StartingHealth);
            int postDestroyHealthInvokes = 0;
            int postDestroyDestroyedInvokes = 0;
            _baseHealthService.OnHealthChanged += (_) => postDestroyHealthInvokes++;
            _baseHealthService.OnBaseDestroyed += () => postDestroyDestroyedInvokes++;

            _baseHealthService.TakeDamage(10);
            Assert.AreEqual(0, _baseHealthService.CurrentHealth, "Máu phải giữ nguyên ở mức 0.");
            Assert.AreEqual(0, postDestroyHealthInvokes, "Không được gọi thay đổi UI/Event máu nữa.");
            Assert.AreEqual(0, postDestroyDestroyedInvokes, "Không được gọi kiện phá hủy thêm lần nào nữa.");
        }
    }
}