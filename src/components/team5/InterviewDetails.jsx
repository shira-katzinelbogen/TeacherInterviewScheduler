import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Divider,
  Grid,
  IconButton,
  Chip
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import EventIcon from '@mui/icons-material/Event';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import InterviewStatusDisplay from './InterviewStatusDisplay.jsx';
import './InterviewDetails.css';

function InterviewDetails({ interview, open, onClose }) {
  if (!interview) return null;

  const formatDateTime = (dateString) => {
    const date = new Date(dateString);
    return {
      date: date.toLocaleDateString('he-IL', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      }),
      time: date.toLocaleTimeString('he-IL', {
        hour: '2-digit',
        minute: '2-digit'
      })
    };
  };

  const startDateTime = formatDateTime(interview.slotStart);
  const endDateTime = formatDateTime(interview.slotEnd);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      className="interview-details-dialog"
    >
      <DialogTitle className="interview-details-title">
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6" component="div">
            פרטי הריאיון
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent className="interview-details-content">
        {/* Header Section */}
        <Box mb={3}>
          <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
            <Box>
              <Typography variant="h5" component="h2" sx={{ fontWeight: '600', color: '#1565c0', marginBottom: '8px' }}>
                ריאיון {interview.interviewType || 'כללי'}
              </Typography>
              <Chip
                label={interview.interviewType || 'לא צוין'}
                variant="outlined"
                size="small"
                sx={{ height: '24px', fontSize: '0.75rem' }}
              />
            </Box>
            <InterviewStatusDisplay
              status={interview.interviewStatus}
              size="medium"
            />
          </Box>
          <Divider sx={{ opacity: 0.4 }} />
        </Box>

        {/* Main Details Grid */}
        <Grid container spacing={3}>
          {/* Date and Time Section */}
          <Grid item xs={12} md={6}>
            <Box className="detail-section">
              <Typography variant="h6" component="h3" sx={{ fontWeight: '600', color: '#1565c0', marginBottom: '16px' }}>
                זמן ותאריך
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                <Box display="flex" alignItems="center" gap={1}>
                  <EventIcon sx={{ fontSize: '18px', color: '#1976d2' }} />
                  <Typography variant="body1">
                    <strong>תאריך:</strong> {startDateTime.date}
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" gap={1}>
                  <AccessTimeIcon sx={{ fontSize: '18px', color: '#1976d2' }} />
                  <Typography variant="body1">
                    <strong>שעה:</strong> {startDateTime.time} - {endDateTime.time}
                  </Typography>
                </Box>
              </Box>
            </Box>
          </Grid>

          {/* Location Section */}
          {interview.place && (
            <Grid item xs={12} md={6}>
              <Box className="detail-section">
                <Typography variant="h6" component="h3" sx={{ fontWeight: '600', color: '#1565c0', marginBottom: '16px' }}>
                  מיקום
                </Typography>
                <Typography variant="body1" sx={{ fontSize: '0.95rem' }}>
                  {interview.place}
                </Typography>
              </Box>
            </Grid>
          )}

          {/* Additional Details Section */}
          <Grid item xs={12}>
            <Box className="detail-section">
              <Typography variant="h6" component="h3" sx={{ fontWeight: '600', color: '#1565c0', marginBottom: '16px' }}>
                פרטים נוספים
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                <Typography variant="body1">
                  <strong>סוג ריאיון:</strong> {interview.interviewType || 'לא צוין'}
                </Typography>
                <Typography variant="body1">
                  <strong>מזהה ריאיון:</strong> #{interview.id}
                </Typography>
                <Typography variant="body1">
                  <strong>מזהה משבץ:</strong> #{interview.slotId}
                </Typography>
              </Box>
            </Box>
          </Grid>

          {/* Comments Section */}
          {interview.comments && (
            <Grid item xs={12}>
              <Box className="detail-section">
                <Typography variant="h6" component="h3" sx={{ fontWeight: '600', color: '#1565c0', marginBottom: '16px' }}>
                  הערות
                </Typography>
                <Box className="comments-box">
                  <Typography variant="body1" sx={{ lineHeight: '1.6', fontSize: '0.95rem' }}>
                    {interview.comments}
                  </Typography>
                </Box>
              </Box>
            </Grid>
          )}
        </Grid>
      </DialogContent>

      <DialogActions className="interview-details-actions">
        <Button
          onClick={onClose}
          variant="contained"
          sx={{
            background: 'linear-gradient(135deg, #1976d2, #1565c0)',
            textTransform: 'none',
            fontSize: '0.95rem',
            fontWeight: '600',
            minWidth: '150px'
          }}
        >
          חזרה לרשימה
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export default InterviewDetails;