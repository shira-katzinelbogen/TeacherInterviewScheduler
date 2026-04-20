import { useState } from 'react';
import { Stack, Typography } from '@mui/material';
import PageShell from '../../../components/PageShell/PageShell';
import StudentAvailabilityCalendar from '../../../components/Scheduling/StudentAvailabilityCalendar';

export default function StudentAvailabilityPage() {
  const [value, setValue] = useState({});

  return (
    <PageShell>
      <Stack spacing={2}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 800 }}>
          זמינות לראיונות
        </Typography>
        <StudentAvailabilityCalendar value={value} onChange={setValue} minDate={new Date()} />
      </Stack>
    </PageShell>
  );
}

