using CSUnit.Exceptions;

namespace CSUnit.Assertions
{

    public static class Assertions
    {
        public static void assertEquals(object expected, object actual)
        {
            if (!Equals(expected, actual))
                throw new AssertionFailedException($"Expected <{expected}>, but was <{actual}>.");
        }

        public static void assertNotEquals(object unexpected, object actual)
        {
            if (Equals(unexpected, actual))
                throw new AssertionFailedException($"Expected values to be different, but both were <{actual}>.");
        }

        public static void assertTrue(bool condition)
        {
            if (!condition)
                throw new AssertionFailedException("Expected condition to be true.");
        }

        public static void assertFalse(bool condition)
        {
            if (condition)
                throw new AssertionFailedException("Expected condition to be false.");
        }

        public static void assertNull(object? obj)
        {
            if (obj != null)
                throw new AssertionFailedException($"Expected null, but was <{obj}>.");
        }

        public static void assertNotNull(object? obj)
        {
            if (obj == null)
                throw new AssertionFailedException("Expected non-null object.");
        }

        public static void assertSame(object expected, object actual)
        {
            if (!ReferenceEquals(expected, actual))
                throw new AssertionFailedException("Expected same instance, but got different references.");
        }

        public static void assertNotSame(object unexpected, object actual)
        {
            if (ReferenceEquals(unexpected, actual))
                throw new AssertionFailedException("Expected different instances, but got the same reference.");
        }

        public static void assertThrows<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new AssertionFailedException($"Expected {typeof(T).Name} to be thrown, but {ex.GetType().Name} was thrown.");
            }
            throw new AssertionFailedException($"Expected {typeof(T).Name} to be thrown, but nothing was thrown.");
        }

        public static void assertTimeout(TimeSpan timeout, Action action)
        {
            var task = Task.Run(action);
            if (!task.Wait(timeout))
            {
                throw new AssertionFailedException($"Execution exceeded timeout of {timeout.TotalMilliseconds}ms.");
            }
        }
    }
}