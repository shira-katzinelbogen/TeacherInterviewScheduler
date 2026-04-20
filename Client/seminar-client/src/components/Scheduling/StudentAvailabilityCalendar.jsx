import { useMemo, useState } from 'react';
import {
  Add as AddIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import {
  Box,
  Button,
  Chip,
  Divider,
  Drawer,
  IconButton,
  Paper,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';

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
 * - value: map of YYYY-MM-DD to slots:
 *   { [dateKey: string]: Array<{ start: "HH:MM", end: "HH:MM", status?: 0|1, reasonStudent?: string, reasonStatus?: 0|1 }> }
 * - onChange(nextValue)
 * - minDate?: Date (e.g. new Date())
 */
export default function StudentAvailabilityCalendar({ value, onChange, minDate }) {
  const availabilityByDate = value || {};
  const [cursorMonth, setCursorMonth] = useState(() => startOfMonth(clampToTodayIfNeeded(new Date(), minDate)));
  const [selectedDateKey, setSelectedDateKey] = useState(() => toIsoDateKey(clampToTodayIfNeeded(new Date(), minDate)));
  const [drawerOpen, setDrawerOpen] = useState(false);

  const selectedDate = useMemo(() => parseIsoDateKey(selectedDateKey), [selectedDateKey]);
  const selectedSlots = useMemo(() => normalizeSlots(availabilityByDate[selectedDateKey]), [availabilityByDate, selectedDateKey]);

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

  function setSlotsForSelected(nextSlots) {
    const cleaned = normalizeSlots(nextSlots);
    const next = { ...availabilityByDate, [selectedDateKey]: cleaned };
    onChange?.(next);
  }

  function removeSlot(idx) {
    const next = selectedSlots.filter((_, i) => i !== idx);
    setSlotsForSelected(next);
  }

  function updateSlot(idx, patch) {
    const next = selectedSlots.map((s, i) => (i === idx ? { ...s, ...patch } : s));
    setSlotsForSelected(next);
  }

  function addSlot(slot) {
    const next = [...selectedSlots, slot].sort((a, b) => compareTime(a.start, b.start));
    setSlotsForSelected(next);
  }

  const [newStart, setNewStart] = useState(defaultSlot().start);
  const [newEnd, setNewEnd] = useState(defaultSlot().end);

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
      status: AvailabilityStatus.Available,
      reasonStudent: '',
      reasonStatus: AvailabilityReasonKind.Personal,
    });
  }

  function quickAdd(range) {
    if (range === 'morning')
      addSlot({ start: '09:00', end: '12:00', status: AvailabilityStatus.Available, reasonStudent: '', reasonStatus: 0 });
    if (range === 'noon')
      addSlot({ start: '12:00', end: '15:00', status: AvailabilityStatus.Available, reasonStudent: '', reasonStatus: 0 });
    if (range === 'evening')
      addSlot({ start: '15:00', end: '18:00', status: AvailabilityStatus.Available, reasonStudent: '', reasonStatus: 0 });
  }

  function daySummary(date) {
    if (!date) return null;
    const key = toIsoDateKey(date);
    const slots = normalizeSlots(availabilityByDate[key]);
    if (slots.length === 0) return { kind: 'empty', text: 'לא הוגדר' };

    const availableCount = slots.filter((s) => s.status === AvailabilityStatus.Available).length;
    const unavailableCount = slots.length - availableCount;

    if (availableCount === 0 && unavailableCount > 0) return { kind: 'warn', text: `${unavailableCount} לא זמינים` };
    if (unavailableCount === 0) return { kind: 'some', text: `${availableCount} זמינים` };
    return { kind: 'some', text: `${availableCount} זמינים · ${unavailableCount} לא זמינים` };
  }

  const headers = weekdayHeadersHe();
  const monthLabel = formatHebrewMonthYear(cursorMonth);

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1} flexWrap="wrap">
        <Typography variant="h5" component="h2" sx={{ fontWeight: 700 }}>
          זמינות לראיונות
        </Typography>

        <Stack direction="row" alignItems="center" gap={1}>
          <Tooltip title="חודש קודם">
            <IconButton aria-label="חודש קודם" onClick={() => setCursorMonth((m) => addMonths(m, -1))}>
              <ChevronRightIcon />
            </IconButton>
          </Tooltip>

          <Typography variant="body1" sx={{ fontWeight: 700 }}>
            {monthLabel}
          </Typography>

          <Tooltip title="חודש הבא">
            <IconButton aria-label="חודש הבא" onClick={() => setCursorMonth((m) => addMonths(m, 1))}>
              <ChevronLeftIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      </Stack>

      <Paper elevation={0} sx={{ p: 2 }}>
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
              sx={{ textAlign: 'center', color: 'text.secondary', fontWeight: 700, py: 0.5 }}
            >
              {h}
            </Typography>
          ))}

          {monthGrid.map((date, idx) => {
            if (!date) {
              return <Box key={`empty-${idx}`} sx={{ height: 92 }} />;
            }

            const key = toIsoDateKey(date);
            const isSelected = key === selectedDateKey && drawerOpen;
            const isToday = key === toIsoDateKey(new Date());
            const summary = daySummary(date);

            let disabled = false;
            if (minDate) {
              const dayStart = new Date(date.getFullYear(), date.getMonth(), date.getDate());
              const minStart = new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate());
              disabled = dayStart < minStart;
            }

            return (
              <Button
                key={key}
                onClick={() => openDay(date)}
                disabled={disabled}
                variant={isSelected ? 'contained' : 'outlined'}
                sx={{
                  height: 92,
                  borderRadius: 3,
                  alignItems: 'stretch',
                  p: 1,
                  textAlign: 'right',
                  justifyContent: 'flex-start',
                  ...(isSelected && { boxShadow: 1 }),
                }}
              >
                <Stack spacing={0.75} sx={{ width: '100%' }}>
                  <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
                    <Typography variant="body1" sx={{ fontWeight: 800 }}>
                      {date.getDate()}
                    </Typography>
                    {isToday ? <Chip size="small" label="היום" /> : null}
                  </Stack>

                  {summary ? (
                    <Typography
                      variant="body2"
                      sx={{
                        color:
                          summary.kind === 'empty'
                            ? 'text.secondary'
                            : summary.kind === 'warn'
                              ? 'text.secondary'
                              : 'text.primary',
                      }}
                    >
                      {summary.text}
                    </Typography>
                  ) : null}
                </Stack>
              </Button>
            );
          })}
        </Box>
      </Paper>

      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        PaperProps={{ sx: { width: { xs: '100%', sm: 420 }, p: 2 } }}
      >
        <Stack spacing={2} sx={{ height: '100%' }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
            <Stack spacing={0.25}>
              <Typography variant="h6" sx={{ fontWeight: 800 }}>
                {formatHebrewLongDate(selectedDate)}
              </Typography>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                ערכי זמינות לפי חלונות זמן
              </Typography>
            </Stack>

            <IconButton aria-label="סגירה" onClick={() => setDrawerOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Stack>

          <Divider />

          <Stack spacing={1.25}>
            <Typography variant="body1" sx={{ fontWeight: 700 }}>
              הוספת חלון זמן
            </Typography>

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
                החלונות שלי
              </Typography>
              <Chip size="small" label={`${selectedSlots.length} חלונות`} />
            </Stack>

            {selectedSlots.length === 0 ? (
              <Paper elevation={0} sx={{ p: 2 }}>
                <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                  עדיין לא הוגדרה זמינות ביום הזה. הוסיפי חלון זמן כדי שמנהל/ת הראיונות יוכל/תוכל לשבץ אותך.
                </Typography>
              </Paper>
            ) : (
              <Stack spacing={1}>
                {selectedSlots.map((slot, idx) => (
                  <Paper key={`${slot.start}-${slot.end}-${idx}`} elevation={0} sx={{ p: 1.5 }}>
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

