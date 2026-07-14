import { Alert, Button, Snackbar } from '@mui/material';
import { ToastsContext, createToastsContext } from '../contexts';

export default function ToastsContextProvider({ children }: { children: React.ReactNode }): React.ReactElement {
  const { toasts, dispatchToast, dismissToast } = createToastsContext();

  const toast = toasts[0];

  return (
    <ToastsContext.Provider value={dispatchToast}>
      {toast && (toast.type
        ? <Snackbar
            key={toast.id}
            open
            autoHideDuration={5000}
            onClose={() => dismissToast(toast.id)}
          >
            <Alert
              severity={toast.type}
              action={<Button color="inherit" size="small" onClick={() => dismissToast(toast.id)}>Dismiss</Button>}
              sx={{ minWidth: '288px' }}
            >
              {toast.message}
            </Alert>
          </Snackbar>
        : <Snackbar
            key={toast.id}
            open
            message={toast.message}
            action={<Button color="inherit" size="small" onClick={() => dismissToast(toast.id)}>Dismiss</Button>}
            // autoHideDuration={5000}
            onClose={() => dismissToast(toast.id)}
            slotProps={{
            content: {
              sx: {
                backgroundColor: (theme) => theme.palette.background.paper,
                color: (theme) => theme.palette.text.primary,
              },
            },
          }}
          />
      )}
      {children}
    </ToastsContext.Provider>
  );
};