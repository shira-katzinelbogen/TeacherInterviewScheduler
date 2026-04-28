import { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  List,
  ListItem,
  CircularProgress,
  Alert,
  Divider,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Chip
} from '@mui/material';
import FilterListIcon from '@mui/icons-material/FilterList';
import SortIcon from '@mui/icons-material/Sort';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import EventIcon from '@mui/icons-material/Event';
import ChatIcon from '@mui/icons-material/Chat';
import { fetchFromGateway } from '../../api.js';
import InterviewDetails from './InterviewDetails.jsx';
import InterviewStatusDisplay from './InterviewStatusDisplay.jsx';
import { mockScheduledInterviews } from './mockData.js';
import './MyScheduledInterviews.css';

function MyScheduledInterviews() {
  const [interviews, setInterviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedInterview, setSelectedInterview] = useState(null);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState('all');
  const [sortBy, setSortBy] = useState('date-desc');

  // Hardcoded student ID for now - will be replaced with auth context later
  const studentId = 1;

  useEffect(() => {
    fetchScheduledInterviews();
  }, []);

  const fetchScheduledInterviews = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await fetchFromGateway(`/api/students/${studentId}/scheduled-interviews`);
      setInterviews(data);
    } catch (err) {
      console.error('Error fetching interviews:', err);
      // Use mock data when API fails
      console.warn('⚠️ API unavailable - using mock data for demonstration');
      setInterviews(mockScheduledInterviews);
      setError(null);
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetails = (interview) => {
    setSelectedInterview(interview);
    setDetailsOpen(true);
  };

  const handleCloseDetails = () => {
    setDetailsOpen(false);
    setSelectedInterview(null);
  };

  // Filter and sort interviews
  const filteredAndSortedInterviews = useMemo(() => {
    let filtered = [...interviews];

    // Apply status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(
        interview => interview.interviewStatus?.toLowerCase() === statusFilter.toLowerCase()
      );
    }

    // Apply sorting
    filtered.sort((a, b) => {
      switch (sortBy) {
        case 'date-asc':
          return new Date(a.slotStart) - new Date(b.slotStart);
        case 'date-desc':
          return new Date(b.slotStart) - new Date(a.slotStart);
        case 'status':
          return (a.interviewStatus || '').localeCompare(b.interviewStatus || '');
        case 'type':
          return (a.interviewType || '').localeCompare(b.interviewType || '');
        default:
          return 0;
      }
    });

    return filtered;
  }, [interviews, statusFilter, sortBy]);

  const formatDateTime = (dateString) => {
    const date = new Date(dateString);
    return {
      date: date.toLocaleDateString('he-IL'),
      time: date.toLocaleTimeString('he-IL', { hour: '2-digit', minute: '2-digit' })
    };
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box className="my-scheduled-interviews">
      {/* Filters Section */}
      <Box className="filters-container" mb={3}>
        <Typography variant="subtitle2" sx={{ fontWeight: '600', marginBottom: '16px', color: '#1565c0' }}>
          סנן ומיין
        </Typography>
        
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <FilterListIcon sx={{ fontSize: '24px', color: '#1976d2' }} />
            <FormControl sx={{ minWidth: 180 }} size="small">
              <InputLabel id="status-filter-label">סנן לפי סטטוס</InputLabel>
              <Select
                labelId="status-filter-label"
                value={statusFilter}
                label="סנן לפי סטטוס"
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <MenuItem value="all">כל הסטטוסים</MenuItem>
                <MenuItem value="scheduled">מתוכנן</MenuItem>
                <MenuItem value="completed">הושלם</MenuItem>
                <MenuItem value="cancelled">בוטל</MenuItem>
              </Select>
            </FormControl>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <SortIcon sx={{ fontSize: '24px', color: '#1976d2' }} />
            <FormControl sx={{ minWidth: 180 }} size="small">
              <InputLabel id="sort-label">מיין לפי</InputLabel>
              <Select
                labelId="sort-label"
                value={sortBy}
                label="מיין לפי"
                onChange={(e) => setSortBy(e.target.value)}
              >
                <MenuItem value="date-desc">תאריך - חדש לישן</MenuItem>
                <MenuItem value="date-asc">תאריך - ישן לחדש</MenuItem>
                <MenuItem value="status">לפי סטטוס</MenuItem>
                <MenuItem value="type">לפי סוג ריאיון</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </Stack>
      </Box>

      {/* Results Summary */}
      {interviews.length > 0 && (
        <Typography variant="body2" sx={{ color: '#666', marginBottom: '20px', fontWeight: '500' }}>
          {filteredAndSortedInterviews.length === 0
            ? `מוצגים 0 מתוך ${interviews.length} ריאיונות (ריאיונות לא תואמים לסנן)`
            : `מוצגים ${filteredAndSortedInterviews.length} מתוך ${interviews.length} ריאיונות`}
        </Typography>
      )}

      {/* Empty States */}
      {interviews.length === 0 ? (
        <Box className="interviews-empty">
          <Box sx={{ textAlign: 'center' }}>
            <EventIcon sx={{ fontSize: '48px', color: '#ccc', marginBottom: '16px' }} />
            <Typography variant="h6" sx={{ color: '#999', marginBottom: '8px' }}>
              אין ריאיונות מתוכננים כרגע
            </Typography>
            <Typography variant="body2" sx={{ color: '#bbb' }}>
              כל הריאיונות שלך יופיעו כאן
            </Typography>
          </Box>
        </Box>
      ) : filteredAndSortedInterviews.length === 0 ? (
        <Box className="interviews-empty">
          <Box sx={{ textAlign: 'center' }}>
            <FilterListIcon sx={{ fontSize: '48px', color: '#ccc', marginBottom: '16px' }} />
            <Typography variant="h6" sx={{ color: '#999', marginBottom: '8px' }}>
              לא נמצאו ריאיונות
            </Typography>
            <Typography variant="body2" sx={{ color: '#bbb' }}>
              לא נמצאו ריאיונות התואמים לסנן שנבחר. נסו לשנות את הסנן או הסדר.
            </Typography>
          </Box>
        </Box>
      ) : (
        <List className="interviews-list">
          {filteredAndSortedInterviews.map((interview) => {
            const startDateTime = formatDateTime(interview.slotStart);
            const endDateTime = formatDateTime(interview.slotEnd);
            const interviewTypeLabel = interview.interviewType ? `ריאיון ${interview.interviewType}` : 'ריאיון כללי';

            return (
              <ListItem key={interview.id} disablePadding sx={{ mb: 2 }}>
                <Card className="interview-card" sx={{ width: '100%' }}>
                  <CardContent>
                    {/* Card Header - Title and Status */}
                    <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
                      <Box>
                        <Typography variant="h6" component="h3" sx={{ fontWeight: '600', color: '#1565c0' }}>
                          {interviewTypeLabel}
                        </Typography>
                        <Chip
                          label={interview.interviewType}
                          size="small"
                          variant="outlined"
                          sx={{ marginTop: '4px', height: '24px', fontSize: '0.75rem' }}
                        />
                      </Box>
                      <InterviewStatusDisplay
                        status={interview.interviewStatus}
                        size="medium"
                      />
                    </Box>

                    <Divider sx={{ my: 2, opacity: 0.5 }} />

                    {/* Card Body - Interview Details */}
                    <Box className="interview-meta">
                      {/* Date */}
                      <Box display="flex" alignItems="center" gap={1.5}>
                        <EventIcon sx={{ fontSize: '20px', color: '#1976d2' }} />
                        <Typography variant="body2" sx={{ color: '#444', fontWeight: '500' }}>
                          {startDateTime.date}
                        </Typography>
                      </Box>

                      {/* Time */}
                      <Box display="flex" alignItems="center" gap={1.5}>
                        <AccessTimeIcon sx={{ fontSize: '20px', color: '#1976d2' }} />
                        <Typography variant="body2" sx={{ color: '#444', fontWeight: '500' }}>
                          {startDateTime.time} - {endDateTime.time}
                        </Typography>
                      </Box>

                      {/* Location */}
                      {interview.place && (
                        <Box display="flex" alignItems="center" gap={1.5}>
                          <LocationOnIcon sx={{ fontSize: '20px', color: '#1976d2' }} />
                          <Typography variant="body2" sx={{ color: '#444', fontWeight: '500' }}>
                            {interview.place}
                          </Typography>
                        </Box>
                      )}

                      {/* Comments */}
                      {interview.comments && (
                        <Box display="flex" alignItems="flex-start" gap={1.5}>
                          <ChatIcon sx={{ fontSize: '20px', color: '#1976d2', marginTop: '2px' }} />
                          <Typography variant="body2" sx={{ color: '#666', fontStyle: 'italic', lineHeight: '1.5' }}>
                            {interview.comments}
                          </Typography>
                        </Box>
                      )}
                    </Box>

                    {/* Card Footer - Action Button */}
                    <Box mt={3}>
                      <Button
                        variant="contained"
                        fullWidth
                        onClick={() => handleViewDetails(interview)}
                        className="view-details-btn"
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.95rem'
                        }}
                      >
                        צפה בפרטים המלאים
                      </Button>
                    </Box>
                  </CardContent>
                </Card>
              </ListItem>
            );
          })}
        </List>
      )}

      <InterviewDetails
        interview={selectedInterview}
        open={detailsOpen}
        onClose={handleCloseDetails}
      />
    </Box>
  );
}

export default MyScheduledInterviews;