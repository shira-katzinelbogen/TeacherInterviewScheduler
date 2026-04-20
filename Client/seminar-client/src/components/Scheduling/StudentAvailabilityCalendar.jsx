import { useEffect, useMemo, useState } from 'react';
import {
  Add as AddIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import {
  alpha,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Drawer,
  IconButton,
  Paper,
  Stack,
  Switch,
  TextField,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';

const AvailabilityStatus = {
  Available: 0,
  Unavailable: 1,
};

const AvailabilityReasonKind = {
  Personal: 0,
  Interview: 1,
};

function pad2(n) {
  return String(n).padStart(2, '0');
}

function toIsoDateKey(date) {
  const y = date.getFullYear();
  const m = pad2(date.getMonth() + 1);
  const d = pad2(date.getDate());
  return `${y}-${m}-${d}`;
}

function parseIsoDateKey(isoKey) {
  const [y, m, d] = isoKey.split('-').map((x) => Number(x));
  return new Date(y, (m || 1) - 1, d || 1);
}

function startOfMonth(date) {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

function addMonths(date, delta) {
  return new Date(date.getFullYear(), date.getMonth() + delta, 1);
}

function daysInMonth(date) {
  const y = date.getFullYear();
  const m = date.getMonth();
  return new Date(y, m + 1, 0).getDate();
}

function compareTime(a, b) {
  // a/b: "HH:MM"
  if (!a || !b) return 0;
  return a.localeCompare(b);
}

function formatHebrewLongDate(date) {
  try {
    return new Intl.DateTimeFormat('he-IL', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }).format(
      date
    );
  } catch {
    return date.toLocaleDateString();
  }
}

function formatHebrewMonthYear(date) {
  try {
    return new Intl.DateTimeFormat('he-IL', { month: 'long', year: 'numeric' }).format(date);
  } catch {
    return date.toLocaleDateString();
  }
}

function weekdayHeadersHe() {
  // Sunday -> Saturday (he-IL)
  return ['א׳', 'ב׳', 'ג׳', 'ד׳', 'ה׳', 'ו׳', 'ש׳'];
}

function getSundayBasedWeekdayIndex(date) {
  // JS: 0=Sunday ... 6=Saturday
  return date.getDay();
}

function clampToTodayIfNeeded(date, minDate) {
  if (!minDate) return date;
  return date < minDate ? minDate : date;
}

function normalizeSlots(slots) {
  const safe = Array.isArray(slots) ? slots : [];
  return safe
    .filter((s) => s && typeof s.start === 'string' && typeof s.end === 'string')
    .map((s) => ({
      id: s.id ?? s.studentAvailabilityId ?? s.studentAvailabilityID ?? s.studentAvailabilityId ?? undefined,
      start: s.start,
      end: s.end,
      status: Number.isFinite(s.status) ? s.status : AvailabilityStatus.Available,
      reasonStudent: typeof s.reasonStudent === 'string' ? s.reasonStudent : '',
      reasonStatus: Number.isFinite(s.reasonStatus) ? s.reasonStatus : AvailabilityReasonKind.Personal,
    }))
    .sort((a, b) => compareTime(a.start, b.start));
}

function normalizeDayEntry(entry) {
  // Supported shapes:
  // 1) { [dateKey]: Slot[] }  (legacy)
  // 2) { [dateKey]: { dayStatus, dayReasonStudent, dayReasonStatus, slots } }
  // Default UX: day is Available, and student adds "exceptions" as needed.
  if (Array.isArray(entry)) {
    return {
      dayStatus: AvailabilityStatus.Available,
      dayReasonStudent: '',
      dayReasonStatus: AvailabilityReasonKind.Personal,
      slots: normalizeSlots(entry),
    };
  }

  const dayStatus = Number.isFinite(entry?.dayStatus) ? entry.dayStatus : AvailabilityStatus.Available;
  const dayReasonStudent = typeof entry?.dayReasonStudent === 'string' ? entry.dayReasonStudent : '';
  const dayReasonStatus = Number.isFinite(entry?.dayReasonStatus) ? entry.dayReasonStatus : AvailabilityReasonKind.Personal;
  const slots = normalizeSlots(entry?.slots);

  return { dayStatus, dayReasonStudent, dayReasonStatus, slots };
}

function slotLabel(slot) {
  return `${slot.start}–${slot.end}`;
}

function defaultSlot() {
  return { start: '09:00', end: '10:00' };
}

/**
 * StudentAvailabilityCalendar
 *
 * Props:
 * - value:
 *   - legacy: { [dateKey: string]: Slot[] }
 *   - preferred: {
 *       [dateKey: string]: {
 *         dayStatus?: 0|1,
 *         dayReasonStudent?: string,
 *         dayReasonStatus?: 0|1,
 *         slots?: Slot[]
 *       }
 *     }
 * - onChange(nextValue)
 * - minDate?: Date (e.g. new Date())
 */
export default function StudentAvailabilityCalendar({ value, onChange, minDate }) {
  const theme = useTheme();
  const availabilityByDate = value || {};
  const [cursorMonth, setCursorMonth] = useState(() => startOfMonth(clampToTodayIfNeeded(new Date(), minDate)));
  const [selectedDateKey, setSelectedDateKey] = useState(() => toIsoDateKey(clampToTodayIfNeeded(new Date(), minDate)));
  const [drawerOpen, setDrawerOpen] = useState(false);

  const selectedDate = useMemo(() => parseIsoDateKey(selectedDateKey), [selectedDateKey]);
  const selectedDayEntry = useMemo(
    () => normalizeDayEntry(availabilityByDate[selectedDateKey]),
    [availabilityByDate, selectedDateKey]
  );
  const selectedSlots = selectedDayEntry.slots;
  const selectedDayStatus = selectedDayEntry.dayStatus;
  const selectedDayReasonStudent = selectedDayEntry.dayReasonStudent;
  const selectedDayReasonStatus = selectedDayEntry.dayReasonStatus;

  const monthGrid = useMemo(() => {
    const monthStart = startOfMonth(cursorMonth);
    const dim = daysInMonth(monthStart);
    const offset = getSundayBasedWeekdayIndex(monthStart); // leading empty cells
    const cells = [];
    for (let i = 0; i < offset; i += 1) cells.push(null);
    for (let day = 1; day <= dim; day += 1) {
      const date = new Date(monthStart.getFullYear(), monthStart.getMonth(), day);
      cells.push(date);
    }
    while (cells.length % 7 !== 0) cells.push(null);
    return cells;
  }, [cursorMonth]);

  function openDay(date) {
    const key = toIsoDateKey(date);
    if (minDate) {
      const dayStart = new Date(date.getFullYear(), date.getMonth(), date.getDate());
      const minStart = new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate());
      if (dayStart < minStart) return;
    }
    setSelectedDateKey(key);
    setDrawerOpen(true);
  }

  function setSelectedDayEntry(patch) {
    const nextEntry = { ...selectedDayEntry, ...patch };
    const next = { ...availabilityByDate, [selectedDateKey]: nextEntry };
    onChange?.(next);
  }

  function removeSlot(idx) {
    const next = selectedSlots.filter((_, i) => i !== idx);
    setSelectedDayEntry({ slots: normalizeSlots(next) });
  }

  function updateSlot(idx, patch) {
    const next = selectedSlots.map((s, i) => (i === idx ? { ...s, ...patch } : s));
    setSelectedDayEntry({ slots: normalizeSlots(next) });
  }

  function addSlot(slot) {
    const next = [...selectedSlots, slot].sort((a, b) => compareTime(a.start, b.start));
    setSelectedDayEntry({ slots: normalizeSlots(next) });
  }

  const [newStart, setNewStart] = useState(defaultSlot().start);
  const [newEnd, setNewEnd] = useState(defaultSlot().end);
  const [newIsAvailable, setNewIsAvailable] = useState(true);

  // When toggling whole-day status, make "add slot" default to the opposite (exceptions).
  // Whole day Available -> add Unavailable exceptions (block hours).
  // Whole day Unavailable -> add Available exceptions (allow an hour).
  // We only auto-adjust when drawer opens or the selected date changes.
  useEffect(() => {
    if (!drawerOpen) return;
    setNewIsAvailable(selectedDayStatus === AvailabilityStatus.Unavailable);
  }, [selectedDayStatus, selectedDateKey, drawerOpen]);

  const newSlotError = useMemo(() => {
    if (!newStart || !newEnd) return 'בחר/י שעה התחלה וסיום';
    if (compareTime(newStart, newEnd) >= 0) return 'שעת סיום חייבת להיות אחרי שעת ההתחלה';
    return '';
  }, [newStart, newEnd]);

  function commitAddNewSlot() {
    if (newSlotError) return;
    addSlot({
      start: newStart,
      end: newEnd,
      status: newIsAvailable ? AvailabilityStatus.Available : AvailabilityStatus.Unavailable,
      reasonStudent: '',
      reasonStatus: AvailabilityReasonKind.Personal,
    });
  }

  function quickAdd(range) {
    if (range === 'morning')
      addSlot({
        start: '09:00',
        end: '12:00',
        status: newIsAvailable ? AvailabilityStatus.Available : AvailabilityStatus.Unavailable,
        reasonStudent: '',
        reasonStatus: 0,
      });
    if (range === 'noon')
      addSlot({
        start: '12:00',
        end: '15:00',
        status: newIsAvailable ? AvailabilityStatus.Available : AvailabilityStatus.Unavailable,
        reasonStudent: '',
        reasonStatus: 0,
      });
    if (range === 'evening')
      addSlot({
        start: '15:00',
        end: '18:00',
        status: newIsAvailable ? AvailabilityStatus.Available : AvailabilityStatus.Unavailable,
        reasonStudent: '',
        reasonStatus: 0,
      });
  }

  function daySummary(date) {
    if (!date) return null;
    const key = toIsoDateKey(date);
    const entry = normalizeDayEntry(availabilityByDate[key]);
    const slots = entry.slots;
    const dayStatus = entry.dayStatus;

    const availableCount = slots.filter((s) => s.status === AvailabilityStatus.Available).length;
    const unavailableCount = slots.length - availableCount;

    if (dayStatus === AvailabilityStatus.Available) {
      if (slots.length === 0) return { kind: 'some', text: 'זמין כל היום' };
      return { kind: 'some', text: `זמין · ${unavailableCount} חסומים` };
    }

    // Whole day Unavailable
    if (slots.length === 0) return { kind: 'warn', text: 'לא זמין כל היום' };
    return { kind: 'some', text: `לא זמין · ${availableCount} זמינים` };
  }

  const headers = weekdayHeadersHe();
  const monthLabel = formatHebrewMonthYear(cursorMonth);

  const monthStats = useMemo(() => {
    const monthStart = startOfMonth(cursorMonth);
    const dim = daysInMonth(monthStart);
    let totalDays = 0;
    let daysUnavailable = 0;
    let daysWithExceptions = 0;

    for (let day = 1; day <= dim; day += 1) {
      const date = new Date(monthStart.getFullYear(), monthStart.getMonth(), day);
      if (minDate) {
        const dayStart = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        const minStart = new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate());
        if (dayStart < minStart) continue;
      }

      totalDays += 1;
      const entry = normalizeDayEntry(availabilityByDate[toIsoDateKey(date)]);
      if (entry.dayStatus === AvailabilityStatus.Unavailable) daysUnavailable += 1;
      if (entry.slots.length > 0) daysWithExceptions += 1;
    }

    return { totalDays, daysUnavailable, daysWithExceptions };
  }, [availabilityByDate, cursorMonth, minDate]);

  return (
    <Stack spacing={2.5}>
      <Stack direction="row" alignItems="flex-start" justifyContent="space-between" gap={2} flexWrap="wrap">
        <Stack spacing={0.5}>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 800 }}>
            זמינות לראיונות
          </Typography>
          <Typography variant="body2" sx={{ color: 'text.secondary' }}>
            בחרו יום כדי לסמן זמינות יומית או להוסיף חריגות לשעות ספציפיות
          </Typography>
        </Stack>

        <Stack direction="row" alignItems="center" gap={1}>
          <Tooltip title="חודש קודם">
            <IconButton aria-label="חודש קודם" onClick={() => setCursorMonth((m) => addMonths(m, -1))}>
              <ChevronRightIcon />
            </IconButton>
          </Tooltip>

          <Typography variant="body1" sx={{ fontWeight: 800 }}>
            {monthLabel}
          </Typography>

          <Tooltip title="חודש הבא">
            <IconButton aria-label="חודש הבא" onClick={() => setCursorMonth((m) => addMonths(m, 1))}>
              <ChevronLeftIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      </Stack>

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Card variant="outlined" sx={{ flex: 1, borderRadius: 4 }}>
          <CardContent sx={{ pb: 2.5 }}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2} flexWrap="wrap">
              <Stack spacing={0.25}>
                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  סיכום לחודש
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 800 }}>
                  {monthLabel}
                </Typography>
              </Stack>

              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                <Chip size="small" label={`${monthStats.totalDays} ימים זמינים לעריכה`} />
                <Chip size="small" variant="outlined" label={`${monthStats.daysWithExceptions} ימים עם חריגות`} />
                <Chip size="small" color="warning" variant="outlined" label={`${monthStats.daysUnavailable} ימים לא זמינים`} />
              </Stack>
            </Stack>

            <Divider sx={{ my: 2 }} />

            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              <Chip size="small" label="טיפ: זמין כל היום ואז חוסמים שעות" />
              <Chip size="small" variant="outlined" label="טיפ: לא זמין כל היום ואז מוסיפים שעות זמינות" />
            </Stack>
          </CardContent>
        </Card>

        <Card
          variant="outlined"
          sx={{
            width: { xs: '100%', md: 340 },
            borderRadius: 4,
            bgcolor: 'background.paper',
          }}
        >
          <CardContent sx={{ pb: 2.5 }}>
            <Typography variant="body2" sx={{ color: 'text.secondary' }}>
              מקרא
            </Typography>
            <Stack spacing={1.25} sx={{ mt: 1.25 }}>
              <Stack direction="row" alignItems="center" spacing={1}>
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    bgcolor: 'success.main',
                  }}
                />
                <Typography variant="body2">זמין (או רוב היום זמין)</Typography>
              </Stack>
              <Stack direction="row" alignItems="center" spacing={1}>
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    bgcolor: 'warning.main',
                  }}
                />
                <Typography variant="body2">לא זמין (או רוב היום לא זמין)</Typography>
              </Stack>
              <Stack direction="row" alignItems="center" spacing={1}>
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    bgcolor: 'info.main',
                  }}
                />
                <Typography variant="body2">יש חריגות לשעות</Typography>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      </Stack>

      <Paper elevation={0} sx={{ p: { xs: 1.5, sm: 2 }, borderRadius: 4, bgcolor: 'background.paper' }}>
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: 'repeat(7, minmax(0, 1fr))',
            gap: 1,
          }}
        >
          {headers.map((h) => (
            <Typography
              key={h}
              variant="body2"
              sx={{ textAlign: 'center', color: 'text.secondary', fontWeight: 800, py: 0.5 }}
            >
              {h}
            </Typography>
          ))}

          {monthGrid.map((date, idx) => {
            if (!date) {
              return <Box key={`empty-${idx}`} sx={{ height: { xs: 78, sm: 96 } }} />;
            }

            const key = toIsoDateKey(date);
            const isSelected = key === selectedDateKey && drawerOpen;
            const isToday = key === toIsoDateKey(new Date());
            const summary = daySummary(date);
            const entry = normalizeDayEntry(availabilityByDate[key]);

            let disabled = false;
            if (minDate) {
              const dayStart = new Date(date.getFullYear(), date.getMonth(), date.getDate());
              const minStart = new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate());
              disabled = dayStart < minStart;
            }

            const baseBorder = theme.palette.divider;
            const hasExceptions = entry.slots.length > 0;
            const dayStatus = entry.dayStatus;
            const dotColor =
              dayStatus === AvailabilityStatus.Unavailable ? theme.palette.warning.main : theme.palette.success.main;

            return (
              <Box key={key} sx={{ position: 'relative' }}>
                <Button
                  onClick={() => openDay(date)}
                  disabled={disabled}
                  variant="text"
                  sx={{
                    width: '100%',
                    height: { xs: 78, sm: 96 },
                    borderRadius: 3,
                    alignItems: 'stretch',
                    p: 1.25,
                    textAlign: 'right',
                    justifyContent: 'flex-start',
                    border: `1px solid ${baseBorder}`,
                    bgcolor: isSelected ? alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.22 : 0.08) : 'transparent',
                    boxShadow: isSelected ? 1 : 0,
                    transition: 'transform 120ms ease, box-shadow 120ms ease, background-color 120ms ease',
                    '&:hover': {
                      bgcolor: alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.18 : 0.06),
                      boxShadow: 1,
                      transform: disabled ? 'none' : 'translateY(-1px)',
                    },
                    '&.Mui-disabled': {
                      opacity: 0.45,
                    },
                  }}
                >
                  <Stack spacing={0.75} sx={{ width: '100%' }}>
                    <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
                      <Stack direction="row" alignItems="center" spacing={1}>
                        <Box
                          sx={{
                            width: 10,
                            height: 10,
                            borderRadius: '50%',
                            bgcolor: dotColor,
                            flex: '0 0 auto',
                          }}
                        />
                        <Typography variant="body1" sx={{ fontWeight: 900 }}>
                          {date.getDate()}
                        </Typography>
                      </Stack>

                      <Stack direction="row" spacing={0.75} alignItems="center">
                        {hasExceptions ? <Chip size="small" variant="outlined" label="חריגות" /> : null}
                        {isToday ? <Chip size="small" label="היום" /> : null}
                      </Stack>
                    </Stack>

                    {summary ? (
                      <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                        {summary.text}
                      </Typography>
                    ) : null}
                  </Stack>
                </Button>
              </Box>
            );
          })}
        </Box>
      </Paper>

      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        PaperProps={{ sx: { width: { xs: '100%', sm: 440 }, p: 2.25 } }}
      >
        <Stack spacing={2} sx={{ height: '100%' }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
            <Stack spacing={0.25}>
              <Typography variant="h6" sx={{ fontWeight: 900 }}>
                {formatHebrewLongDate(selectedDate)}
              </Typography>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                הגדירי זמינות ליום הזה וחריגות לשעות ספציפיות
              </Typography>
            </Stack>

            <IconButton aria-label="סגירה" onClick={() => setDrawerOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Stack>

          <Divider />

          <Stack spacing={1.25}>
            <Paper
              elevation={0}
              sx={{
                p: 1.75,
                borderRadius: 3,
                border: '1px solid',
                borderColor: 'divider',
                bgcolor: 'background.paper',
              }}
            >
              <Stack spacing={1.25}>
                <Typography variant="body1" sx={{ fontWeight: 900 }}>
                  מצב יום
                </Typography>

                <ToggleButtonGroup
                  exclusive
                  value={selectedDayStatus}
                  onChange={(_, next) => {
                    if (next === null || next === undefined) return;
                    setSelectedDayEntry({ dayStatus: next });
                  }}
                  aria-label="מצב יום"
                  fullWidth
                  sx={{
                    '& .MuiToggleButton-root': {
                      flex: 1,
                      borderRadius: 999,
                      fontWeight: 800,
                      py: 1,
                    },
                  }}
                >
                  <ToggleButton value={AvailabilityStatus.Available} aria-label="זמין כל היום">
                    זמין/ה כל היום
                  </ToggleButton>
                  <ToggleButton value={AvailabilityStatus.Unavailable} aria-label="לא זמין כל היום">
                    לא זמין/ה כל היום
                  </ToggleButton>
                </ToggleButtonGroup>

                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  {selectedDayStatus === AvailabilityStatus.Available
                    ? 'אפשר לחסום שעות בודדות באמצעות חריגות מסוג "לא זמין"'
                    : 'אפשר להוסיף שעות בודדות כ"זמין" כדי לאפשר ראיון'}
                </Typography>
              </Stack>
            </Paper>

            {selectedDayStatus === AvailabilityStatus.Unavailable ? (
              <Stack spacing={1}>
                <TextField
                  label="סיבה ליום (אופציונלי)"
                  value={selectedDayReasonStudent}
                  onChange={(e) => setSelectedDayEntry({ dayReasonStudent: e.target.value })}
                  fullWidth
                />

                <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                  <Chip
                    clickable
                    onClick={() => setSelectedDayEntry({ dayReasonStatus: AvailabilityReasonKind.Personal })}
                    variant={selectedDayReasonStatus === AvailabilityReasonKind.Personal ? 'filled' : 'outlined'}
                    label="פרטי"
                  />
                  <Chip
                    clickable
                    onClick={() => setSelectedDayEntry({ dayReasonStatus: AvailabilityReasonKind.Interview })}
                    variant={selectedDayReasonStatus === AvailabilityReasonKind.Interview ? 'filled' : 'outlined'}
                    label="ראיון"
                  />
                </Stack>
              </Stack>
            ) : null}

            <Divider />

            <Typography variant="body1" sx={{ fontWeight: 700 }}>
              הוספת חריגה ליום
            </Typography>

            <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2} flexWrap="wrap">
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                {newIsAvailable ? 'החלון הזה יסומן כזמין' : 'החלון הזה יסומן כלא זמין'}
              </Typography>
              <Stack direction="row" alignItems="center" spacing={1}>
                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  לא זמין
                </Typography>
                <Switch
                  checked={newIsAvailable}
                  onChange={(e) => setNewIsAvailable(e.target.checked)}
                  inputProps={{ 'aria-label': 'סוג החלון' }}
                />
                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  זמין
                </Typography>
              </Stack>
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
              <TextField
                label="התחלה"
                type="time"
                value={newStart}
                onChange={(e) => setNewStart(e.target.value)}
                fullWidth
                InputLabelProps={{ shrink: true }}
                inputProps={{ step: 300 }}
              />
              <TextField
                label="סיום"
                type="time"
                value={newEnd}
                onChange={(e) => setNewEnd(e.target.value)}
                fullWidth
                InputLabelProps={{ shrink: true }}
                inputProps={{ step: 300 }}
                error={Boolean(newSlotError)}
                helperText={newSlotError || ' '}
              />
            </Stack>

            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              <Button startIcon={<AddIcon />} onClick={commitAddNewSlot} disabled={Boolean(newSlotError)} variant="contained">
                הוספה
              </Button>
              <Button onClick={() => quickAdd('morning')} variant="outlined">
                בוקר (09–12)
              </Button>
              <Button onClick={() => quickAdd('noon')} variant="outlined">
                צהריים (12–15)
              </Button>
              <Button onClick={() => quickAdd('evening')} variant="outlined">
                אחה״צ (15–18)
              </Button>
            </Stack>
          </Stack>

          <Divider />

          <Stack spacing={1} sx={{ flex: 1, overflow: 'auto' }}>
            <Stack direction="row" alignItems="center" justifyContent="space-between">
              <Typography variant="body1" sx={{ fontWeight: 700 }}>
                החריגות שלי
              </Typography>
              <Chip size="small" label={`${selectedSlots.length} חלונות`} />
            </Stack>

            {selectedSlots.length === 0 ? (
              <Paper elevation={0} sx={{ p: 2, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  אין חריגות ליום הזה. אם סימנת "זמין כל היום" אפשר להוסיף חריגה "לא זמין" לשעות ספציפיות, ואם סימנת "לא זמין כל היום" אפשר להוסיף חריגה "זמין" לשעות ספציפיות.
                </Typography>
              </Paper>
            ) : (
              <Stack spacing={1}>
                {selectedSlots.map((slot, idx) => (
                  <Paper
                    key={`${slot.start}-${slot.end}-${idx}`}
                    elevation={0}
                    sx={{ p: 1.5, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}
                  >
                    <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
                      <Stack spacing={0.25}>
                        <Typography variant="body1" sx={{ fontWeight: 800 }}>
                          {slotLabel(slot)}
                        </Typography>
                        <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                          {slot.status === AvailabilityStatus.Available ? 'זמין/ה לראיון' : 'לא זמין/ה לראיון'}
                        </Typography>
                      </Stack>

                      <Stack direction="row" alignItems="center" spacing={1}>
                        <Tooltip title={slot.status === AvailabilityStatus.Available ? 'סימון כלא זמין' : 'סימון כזמין'}>
                          <Switch
                            checked={slot.status === AvailabilityStatus.Available}
                            onChange={(e) =>
                              updateSlot(idx, {
                                status: e.target.checked ? AvailabilityStatus.Available : AvailabilityStatus.Unavailable,
                              })
                            }
                            inputProps={{ 'aria-label': 'זמינות' }}
                          />
                        </Tooltip>

                        <Button variant="outlined" color="inherit" onClick={() => removeSlot(idx)} aria-label="מחיקה">
                          מחיקה
                        </Button>
                      </Stack>
                    </Stack>

                    {slot.status === AvailabilityStatus.Unavailable ? (
                      <Stack spacing={1} sx={{ mt: 1 }}>
                        <TextField
                          label="סיבה (אופציונלי)"
                          value={slot.reasonStudent || ''}
                          onChange={(e) => updateSlot(idx, { reasonStudent: e.target.value })}
                          fullWidth
                        />

                        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                          <Chip
                            clickable
                            onClick={() => updateSlot(idx, { reasonStatus: AvailabilityReasonKind.Personal })}
                            variant={slot.reasonStatus === AvailabilityReasonKind.Personal ? 'filled' : 'outlined'}
                            label="פרטי"
                          />
                          <Chip
                            clickable
                            onClick={() => updateSlot(idx, { reasonStatus: AvailabilityReasonKind.Interview })}
                            variant={slot.reasonStatus === AvailabilityReasonKind.Interview ? 'filled' : 'outlined'}
                            label="ראיון"
                          />
                        </Stack>
                      </Stack>
                    ) : null}
                  </Paper>
                ))}
              </Stack>
            )}
          </Stack>

          <Divider />

          <Stack direction="row" spacing={1} justifyContent="flex-start">
            <Button variant="contained" onClick={() => setDrawerOpen(false)}>
              סיום
            </Button>
          </Stack>
        </Stack>
      </Drawer>
    </Stack>
  );
}

