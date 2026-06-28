import {
  Box,
  Switch,
} from '@mui/material';
import {
  DarkMode,
  LightMode,
} from '@mui/icons-material';


export default function ThemeSwitcher({ theme, setTheme }: { theme: 'light' | 'dark'; setTheme: React.Dispatch<React.SetStateAction<'light' | 'dark'>> }): React.ReactElement {
  return (
    <Box
      sx={{
        position: 'fixed',
        top: 16,
        right: 16,
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 1,
      }}
    >
      <LightMode fontSize="small" />
      <Switch
        checked={theme === 'dark'}
        onChange={() => setTheme(theme === 'light' ? 'dark' : 'light')}
        color="default"
      />
      <DarkMode fontSize="small" />
    </Box>
  );
};
