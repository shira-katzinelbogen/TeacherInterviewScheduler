import { useState } from 'react';
import { Paper, Stack, Typography } from '@mui/material';
import PageShell from '../../../components/PageShell/PageShell';
import StudentAvailabilityCalendar from '../../../components/Scheduling/StudentAvailabilityCalendar';

export default function StudentAvailabilityPage() {
  const [value, setValue] = useState({});

  return (
    <PageShell>
      <Stack spacing={2.5}>
        <Paper elevation={0} sx={{ p: { xs: 2, sm: 2.5 }, borderRadius: 4, border: '1px solid', borderColor: 'divider' }}>
          <Stack spacing={0.75}>
            <Typography variant="h4" component="h1" sx={{ fontWeight: 900 }}>
              זמינות לראיונות
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', maxWidth: 720 }}>
              בחרו ימים, סמנו זמינות יומית, והוסיפו חריגות לשעות ספציפיות. אפשר להתחיל מזמינות מלאה ואז לחסום שעות, או להפך.
            </Typography>
          </Stack>
        </Paper>

        <StudentAvailabilityCalendar value={value} onChange={setValue} minDate={new Date()} />
      </Stack>
    </PageShell>
  );
}

