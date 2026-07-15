// ============================================================================
//  Module:         StressTests.Support
//  Description:    Minimal reference circuit breaker used ONLY to validate the
//                  SR30/SR31 policy numbers (trip after 5 consecutive failures,
//                  half-open probe) against performance_stress_manual.md.
//                  NOTE: production code (PlantSafety.Infrastructure) does not
//                  yet implement a circuit breaker / Polly policy for external
//                  dependencies (messaging, blob storage, search) -- this is a
//                  documented gap, not a production defect fixed by this test
//                  suite. This helper exists purely so the SR30/SR31 THRESHOLD
//                  NUMBERS are pinned by an executable test pending the real
//                  Polly-based implementation.
// ============================================================================
namespace PlantSafety.StressTests.Support;

public enum CircuitState { Closed, Open, HalfOpen }

public sealed class SimpleCircuitBreaker(int failureThreshold = 5, TimeSpan? recoveryProbe = null)
{
    private readonly TimeSpan _recoveryProbe = recoveryProbe ?? TimeSpan.FromSeconds(10);
    private int _consecutiveFailures;
    private DateTime? _openedAtUtc;

    public CircuitState State { get; private set; } = CircuitState.Closed;

    public bool TryExecute(Action action, out Exception? error)
    {
        error = null;

        if (State == CircuitState.Open)
        {
            if (_openedAtUtc is not null && DateTime.UtcNow - _openedAtUtc >= _recoveryProbe)
                State = CircuitState.HalfOpen;
            else
                return false;
        }

        try
        {
            action();
            OnSuccess();
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            OnFailure();
            return false;
        }
    }

    private void OnSuccess()
    {
        _consecutiveFailures = 0;
        State = CircuitState.Closed;
        _openedAtUtc = null;
    }

    private void OnFailure()
    {
        _consecutiveFailures++;
        if (_consecutiveFailures >= failureThreshold)
        {
            State = CircuitState.Open;
            _openedAtUtc = DateTime.UtcNow;
        }
    }
}
