using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

// Fully AI Generated if you couldn't tell

/// <summary>
/// Infinite-mode pattern director. It chooses non-repeating pattern cards and
/// reserves each object's catch phase while that object is expected to live.
/// This prevents a later card from silently spawning onto an active phase.
/// </summary>
public class InfiniteSpawnDirector : MonoBehaviour
{
    private const float BounceCycle = 1.88f;
    private const float FirstPaddleLead = 1.75f;
    private const float OpeningSameShooterGap = 1.5f;
    private const float TightTrainGap = 0.16f;
    private const float CompactorWindow = 10f;
    private const int MaxTypeSpawnsPerCompactorWindow = 6;

    [Header("Scene references")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;
    [SerializeField] private RecycleObject crtPrefab;
    [SerializeField] private RecycleObject cdPrefab;
    [SerializeField] private RecycleObject cassettePrefab;

    [Header("Timing")]
    [SerializeField] private float floatTime = 2f;
    [SerializeField] private float firstPaddleLead = FirstPaddleLead;

    [Header("Variation")]
    [SerializeField] private int seed = 2026;
    [SerializeField] private bool randomizeSeed;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text survivalTimeText;

    public static bool HasActiveRun { get; private set; }
    public static float LastSurvivalTime { get; private set; }

    private readonly List<SpawnRequest> _pending = new();
    private readonly List<PhaseReservation> _reservations = new();
    private readonly Queue<int> _recentPatterns = new();
    private readonly float[] _lastSpawnBySide = { float.NegativeInfinity, float.NegativeInfinity };
    private readonly List<float>[] _scheduledTypeTimes = { new(), new(), new() };

    private Bucket[] _buckets;
    private System.Random _random;
    private float _startTime;
    private float _nextCardSpawn;
    private RecycleObjectType _focusType;
    private int _focusCardsRemaining;
    private int _burnStreak;
    private int _coldStreak;
    private bool _configuredInfiniteUi;
    private bool _runEnded;
    private float _finalSurvivalTime;

    private readonly struct SpawnRequest
    {
        public readonly float Time;
        public readonly RecycleObjectType Type;
        public readonly FromDirection Direction;
        public readonly int Fire;

        public SpawnRequest(float time, RecycleObjectType type, FromDirection direction, int fire)
        {
            Time = time;
            Type = type;
            Direction = direction;
            Fire = fire;
        }
    }

    private readonly struct PhaseReservation
    {
        public readonly float Phase;
        public readonly float Until;

        public PhaseReservation(float phase, float until)
        {
            Phase = phase;
            Until = until;
        }
    }

    private readonly struct PatternStep
    {
        public readonly int TypeOffset;
        public readonly bool OppositeSide;
        public readonly int FireOffset;
        public readonly float RestAfter;
        public readonly int TrainCount;

        public PatternStep(int typeOffset, bool oppositeSide, int fireOffset, float restAfter, int trainCount = 1)
        {
            TypeOffset = typeOffset;
            OppositeSide = oppositeSide;
            FireOffset = fireOffset;
            RestAfter = restAfter;
            TrainCount = trainCount;
        }
    }

    private readonly struct PatternCard
    {
        public readonly string Name;
        public readonly PatternStep[] Steps;

        public PatternCard(string name, params PatternStep[] steps)
        {
            Name = name;
            Steps = steps;
        }
    }

    // These are deliberately uneven phrases, not permutations of one rhythm.
    // Type/side are relative to a randomized starting point, so each card has
    // several natural variants without weakening the safety planner.
    private static readonly PatternCard[] Cards =
    {
        new("Ripple", new(0, false, 0, .82f, 2), new(1, true, 1, .94f), new(2, false, 0, 0f)),
        new("Wide zig-zag", new(0, false, 0, 1.18f), new(1, true, 0, .67f), new(2, true, 2, 1.06f), new(0, false, 1, 0f)),
        new("Anchor return", new(0, false, 0, 1.62f), new(1, false, 1, 1.54f), new(2, true, 0, 0f)),
        new("Uneven switch", new(0, false, 0, .72f), new(1, true, 1, 1.31f), new(2, false, 0, .88f), new(1, true, 2, 0f)),
        new("Long ladder", new(0, false, 0, .91f), new(1, true, 1, 1.47f), new(2, false, 0, .63f), new(0, true, 2, 1.22f), new(1, false, 1, 0f)),
        new("Sawtooth", new(2, true, 0, 1.34f), new(0, false, 2, .74f), new(1, false, 0, 1.68f), new(2, true, 1, 0f)),
        new("Three beats", new(1, false, 1, 2.06f, 3), new(2, true, 0, .86f), new(0, false, 2, 0f)),
        new("Cross current", new(0, true, 0, .58f), new(2, false, 1, 1.72f), new(1, true, 0, 1.13f), new(0, true, 2, 0f)),
        new("Long response", new(2, false, 0, 2.28f), new(1, true, 2, .69f), new(0, false, 1, 1.41f), new(2, true, 0, 0f)),
        new("Offbeat", new(1, true, 0, .97f), new(0, false, 1, .52f), new(2, false, 2, 1.89f), new(1, true, 0, 0f)),
        new("Staircase", new(0, false, 0, 1.11f), new(2, true, 1, 1.11f), new(1, false, 2, 1.11f), new(0, true, 0, 0f)),
        new("Slow-fast-slow", new(2, true, 1, 2.14f), new(0, false, 0, .55f), new(1, true, 2, 2.02f), new(2, false, 0, 0f)),
        new("Six-step drift", new(0, false, 0, .79f), new(1, false, 1, 1.58f, 2), new(2, true, 0, .72f), new(1, true, 2, 1.36f), new(0, false, 1, .61f), new(2, true, 0, 0f)),
        new("Mirror ripple", new(1, true, 0, .84f), new(2, false, 2, 1.49f), new(0, true, 0, .93f), new(1, false, 1, 0f)),
        new("Held edge", new(0, false, 2, 1.76f), new(2, false, 0, .76f), new(1, true, 1, 1.26f), new(0, false, 0, 0f)),
        new("Tumble", new(2, true, 0, .66f), new(1, false, 1, 1.04f), new(0, true, 2, .63f), new(2, false, 0, 1.67f), new(1, true, 1, 0f)),
        new("Side-to-side", new(1, false, 0, 1.43f), new(0, true, 1, 1.37f), new(2, false, 0, 1.45f), new(1, true, 2, 0f)),
        new("Broken wave", new(0, true, 1, .57f), new(1, true, 0, 1.91f, 3), new(2, false, 2, .81f), new(0, false, 0, .94f), new(1, true, 1, 0f)),
        new("Breathing room", new(2, false, 0, 2.41f), new(0, true, 1, 1.02f), new(1, false, 2, 0f)),
        new("Quick turn", new(1, true, 0, .71f), new(2, false, 1, .78f), new(0, true, 0, 1.75f), new(1, false, 2, .65f), new(2, true, 1, 0f)),
        new("Diagonal", new(0, false, 1, 1.29f), new(2, true, 0, .61f), new(1, false, 2, 1.48f), new(0, true, 0, 1.08f), new(2, false, 1, 0f)),
        new("Late answer", new(1, true, 0, 2.17f), new(0, false, 2, .73f), new(2, true, 1, 1.22f), new(1, false, 0, 0f)),
        new("Runaround", new(2, false, 0, .95f), new(0, true, 1, 1.63f), new(1, true, 0, .59f), new(2, false, 2, 1.57f), new(0, true, 1, 0f)),
        new("Finale", new(0, true, 2, .68f, 2), new(1, false, 0, 1.26f), new(2, true, 1, .88f), new(0, false, 0, 1.74f), new(1, true, 2, .64f, 3), new(2, false, 1, 0f))
    };

    private void Start()
    {
        _buckets = FindObjectsByType<Bucket>().OrderBy(bucket => bucket.name).ToArray();
        if (leftSpawnPoint == null || rightSpawnPoint == null || crtPrefab == null || cdPrefab == null ||
            cassettePrefab == null || _buckets.Length != 3)
        {
            Debug.LogError("InfiniteSpawnDirector needs both spawn points, all object prefabs, and three buckets.", this);
            enabled = false;
            return;
        }

        _random = new System.Random(randomizeSeed ? Environment.TickCount : seed);
        _startTime = Time.time;
        HasActiveRun = true;
        LastSurvivalTime = 0f;
        _focusType = (RecycleObjectType)_random.Next(0, 3);
        AddOpeningVariant(_random.Next(0, 4));
        _nextCardSpawn = 12f;
        LevelManager.Instance.onLevelEnded.AddListener(HandleLevelEnded);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelEnded.RemoveListener(HandleLevelEnded);
        }
        HasActiveRun = false;
    }

    private void LateUpdate()
    {
        // LevelManager may finish its scene setup after this component's Start
        // when entering from the menu. LateUpdate makes infinite mode win that
        // initialization race exactly once.
        if (!_configuredInfiniteUi && LevelManager.Instance != null)
        {
            LevelManager.Instance.ConfigureInfiniteMode();
            _configuredInfiniteUi = true;
        }
    }

    private void Update()
    {
        var elapsed = _runEnded ? _finalSurvivalTime : Time.time - _startTime;
        UpdateSurvivalText(survivalTimeText, elapsed);
        if (_runEnded) return;
        while (_nextCardSpawn < elapsed + 10f)
        {
            ScheduleCard(_nextCardSpawn);
        }

        while (_pending.Count > 0 && elapsed >= _pending[0].Time)
        {
            Spawn(_pending[0]);
            _pending.RemoveAt(0);
        }
    }

    private void HandleLevelEnded(int _)
    {
        if (_runEnded || LevelManager.Instance.Hearts > 0) return;
        _runEnded = true;
        _finalSurvivalTime = Time.time - _startTime;
        LastSurvivalTime = _finalSurvivalTime;
        PlayerProgress.RecordSurvivalTime(_finalSurvivalTime);
        UpdateSurvivalText(survivalTimeText, _finalSurvivalTime);
    }

    private void UpdateSurvivalText(TMP_Text text, float elapsed)
    {
        text.text = elapsed.ToString("0.00", CultureInfo.InvariantCulture) + "s";
    }

    private void AddOpeningVariant(int variant)
    {
        // Safe timing from Level 4, varied type focus and mirrored direction.
        var primary = (RecycleObjectType)(variant % 3);
        var first = variant < 2 ? FromDirection.Left : FromDirection.Right;
        AddDirect(5.05f, primary, first, FireFor(5.05f));
        AddDirect(6.55f, NextType(primary), Opposite(first), FireFor(6.55f));
        AddDirect(8.05f, NextType(NextType(primary)), first, FireFor(8.05f));
        AddDirect(9.55f, primary, Opposite(first), FireFor(9.55f));
    }

    private void ScheduleCard(float firstSpawn)
    {
        var pattern = Cards[PickPattern()];
        var direction = _random.Next(2) == 0 ? FromDirection.Left : FromDirection.Right;
        var type = FocusType();
        var cursor = firstSpawn;
        var promotedTrainStep = _random.Next(100) < 90 ? _random.Next(pattern.Steps.Length) : -1;

        for (var stepIndex = 0; stepIndex < pattern.Steps.Length; stepIndex++)
        {
            var step = pattern.Steps[stepIndex];
            var stepType = ResolveType(type, step.TypeOffset);
            var stepDirection = step.OppositeSide ? Opposite(direction) : direction;
            var fire = FireFor(cursor);
            if (fire > 0) fire += step.FireOffset;
            var trainCount = Mathf.Max(step.TrainCount, stepIndex == promotedTrainStep ? (_random.Next(100) < 30 ? 3 : 2) : 1);
            var limited = LimitType(stepType, cursor, trainCount);
            stepType = limited.type;
            cursor = Mathf.Max(cursor, limited.time);
            cursor = ScheduleTrain(cursor, stepType, stepDirection, fire, trainCount) + step.RestAfter;
        }

        // Cards vary in both shape and recovery time. The phase planner may
        // extend this further if an active object still owns a requested phase.
        _nextCardSpawn = cursor + 1.1f + (float)_random.NextDouble() * 1.4f;
    }

    private float Schedule(float earliestSpawn, RecycleObjectType type, FromDirection direction, int fire)
    {
        var side = (int)direction;
        var safeLaunchTime = Mathf.Max(earliestSpawn, _lastSpawnBySide[side] + SameShooterGap(earliestSpawn));
        var contact = FindAvailableContact(safeLaunchTime + firstPaddleLead, fire);
        var spawn = contact - firstPaddleLead;
        _lastSpawnBySide[side] = spawn;
        AddRequest(new SpawnRequest(spawn, type, direction, fire));
        return spawn;
    }

    private float ScheduleTrain(float earliestSpawn, RecycleObjectType type, FromDirection direction, int fire, int count)
    {
        var first = Schedule(earliestSpawn, type, direction, fire);
        if (count <= 1) return first;

        // A train is deliberately one easy shared-paddle action from the very
        // beginning: same type, same side, same route, and same fire count.
        var last = first;
        for (var member = 1; member < count; member++)
        {
            last = first + member * TightTrainGap;
            // Triples change only their final item. The first two remain the
            // easy matching train the player has already read.
            var alternate = NextType(type);
            var memberType = count >= 3 && first >= 60f && member == count - 1 &&
                             ScheduledCount(alternate, last) < MaxTypeSpawnsPerCompactorWindow
                ? alternate
                : type;
            AddRequest(new SpawnRequest(last, memberType, direction, fire));
        }
        _lastSpawnBySide[(int)direction] = last;
        return last;
    }

    private float FindAvailableContact(float earliest, int fire)
    {
        RemoveExpiredReservations(earliest);
        var phaseCount = Mathf.Max(2, Mathf.FloorToInt(BounceCycle / RequiredGap(earliest)));
        var cycleStart = Mathf.Floor(earliest / BounceCycle);

        for (var cycle = 0; cycle < 300; cycle++)
        {
            for (var lane = 0; lane < phaseCount; lane++)
            {
                var contact = (cycleStart + cycle) * BounceCycle + lane * BounceCycle / phaseCount;
                if (contact < earliest || !PhaseIsFree(contact))
                {
                    continue;
                }

                _reservations.Add(new PhaseReservation(PhaseOf(contact), contact + ReservationLength(fire, contact)));
                return contact;
            }
        }

        throw new InvalidOperationException("InfiniteSpawnDirector could not find an available phase.");
    }

    private bool PhaseIsFree(float contact)
    {
        var phase = PhaseOf(contact);
        var requiredGap = RequiredGap(contact);
        foreach (var reservation in _reservations)
        {
            if (reservation.Until > contact && PhaseDistance(phase, reservation.Phase) < requiredGap)
            {
                return false;
            }
        }
        return true;
    }

    private void AddDirect(float spawn, RecycleObjectType type, FromDirection direction, int fire)
    {
        _lastSpawnBySide[(int)direction] = spawn;
        var contact = spawn + firstPaddleLead;
        _reservations.Add(new PhaseReservation(PhaseOf(contact), contact + ReservationLength(fire, contact)));
        AddRequest(new SpawnRequest(spawn, type, direction, fire));
    }

    private void AddRequest(SpawnRequest request)
    {
        var index = _pending.BinarySearch(request, SpawnComparer.Instance);
        _pending.Insert(index < 0 ? ~index : index, request);
        _scheduledTypeTimes[(int)request.Type].Add(request.Time);
    }

    private (RecycleObjectType type, float time) LimitType(RecycleObjectType requested, float time, int count)
    {
        var selected = requested;
        var selectedTime = NextAllowedTime(requested, time, count);
        for (var i = 1; i < 3; i++)
        {
            var candidate = (RecycleObjectType)(((int)requested + i) % 3);
            var candidateTime = NextAllowedTime(candidate, time, count);
            if (candidateTime < selectedTime ||
                (Mathf.Approximately(candidateTime, selectedTime) && ScheduledCount(candidate, candidateTime) < ScheduledCount(selected, selectedTime)))
            {
                selected = candidate;
                selectedTime = candidateTime;
            }
        }
        return (selected, selectedTime);
    }

    private float NextAllowedTime(RecycleObjectType type, float time, int count)
    {
        while (ScheduledCount(type, time) + count > MaxTypeSpawnsPerCompactorWindow)
        {
            var oldest = _scheduledTypeTimes[(int)type]
                .Where(spawnTime => spawnTime <= time && spawnTime >= time - CompactorWindow)
                .DefaultIfEmpty(time)
                .Min();
            time = oldest + CompactorWindow + 0.01f;
        }
        return time;
    }

    private int ScheduledCount(RecycleObjectType type, float time)
    {
        return _scheduledTypeTimes[(int)type]
            .Count(spawnTime => spawnTime <= time && spawnTime >= time - CompactorWindow);
    }

    private void Spawn(SpawnRequest request)
    {
        var prefab = request.Type switch
        {
            RecycleObjectType.Crt => crtPrefab,
            RecycleObjectType.Cd => cdPrefab,
            _ => cassettePrefab
        };
        var left = request.Direction == FromDirection.Left;
        var recycleObject = Instantiate(prefab, (left ? leftSpawnPoint : rightSpawnPoint).position, Quaternion.identity);
        AudioPlayer.Instance.PlaySpawnedObject();
        recycleObject.LaunchTowards(_buckets[left ? 0 : 2].transform.position, left ? 0 : 2, floatTime, request.Fire);
    }

    private int PickPattern()
    {
        var pattern = _random.Next(0, Cards.Length);
        while (_recentPatterns.Contains(pattern))
        {
            pattern = _random.Next(0, Cards.Length);
        }
        _recentPatterns.Enqueue(pattern);
        if (_recentPatterns.Count > 2) _recentPatterns.Dequeue();
        return pattern;
    }

    private int FireFor(float time)
    {
        var burns = _random.NextDouble() < 0.5;
        if (_burnStreak >= 2) burns = false;
        if (_coldStreak >= 2) burns = true;

        if (burns)
        {
            _burnStreak++;
            _coldStreak = 0;
        }
        else
        {
            _coldStreak++;
            _burnStreak = 0;
            return 0;
        }

        return _random.Next(1, 5);
    }

    private RecycleObjectType FocusType()
    {
        if (_focusCardsRemaining <= 0)
        {
            _focusType = (RecycleObjectType)(((int)_focusType + 1 + _random.Next(0, 2)) % 3);
            _focusCardsRemaining = 1 + _random.Next(0, 2);
        }
        _focusCardsRemaining--;
        return _focusType;
    }

    private RecycleObjectType ResolveType(RecycleObjectType focus, int offset)
    {
        // A focused card leans toward one bucket without exhausting its whole
        // ten-second allowance before the next card can begin.
        if (offset == 0 || _random.Next(100) < 45) return focus;
        return (RecycleObjectType)(((int)focus + offset) % 3);
    }

    private static float SameShooterGap(float time)
    {
        if (time < 240f) return OpeningSameShooterGap;
        return Mathf.Lerp(OpeningSameShooterGap, 0.05f, Mathf.Clamp01((time - 240f) / 480f));
    }

    private static float RequiredGap(float time)
    {
        if (time < 240f) return 0.36f;
        return Mathf.Lerp(0.36f, 0.05f, Mathf.Clamp01((time - 240f) / 480f));
    }

    private static float ReservationLength(int fire, float time)
    {
        var normal = (fire + 4) * BounceCycle;
        if (time < 240f) return normal;
        return Mathf.Max(BounceCycle * 2f, normal * Mathf.Lerp(1f, 0.2f, Mathf.Clamp01((time - 240f) / 480f)));
    }

    private void RemoveExpiredReservations(float time)
    {
        _reservations.RemoveAll(reservation => reservation.Until <= time);
    }

    private static RecycleObjectType NextType(RecycleObjectType type) => (RecycleObjectType)(((int)type + 1) % 3);
    private static FromDirection Opposite(FromDirection direction) => direction == FromDirection.Left ? FromDirection.Right : FromDirection.Left;
    private static float PhaseOf(float time) => Mathf.Repeat(time, BounceCycle);
    private static float PhaseDistance(float a, float b) => Mathf.Min(Mathf.Abs(a - b), BounceCycle - Mathf.Abs(a - b));

    private sealed class SpawnComparer : IComparer<SpawnRequest>
    {
        public static readonly SpawnComparer Instance = new();
        public int Compare(SpawnRequest a, SpawnRequest b) => a.Time.CompareTo(b.Time);
    }
}
