import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';
import { primitive } from '@primeng/themes/aura/base';
import { AppThemePreset } from 'oip-common';


export const appTheme: AppThemePreset[] = [
  {
    id: 'Oip',
    label: 'Oip',
    preset: definePreset(Aura, {
      components: {
        toolbar: {
          colorScheme: {
            light: {
              root: {
                borderRadius: '0.7rem'
              }
            },
            dark: {
              root: {
                borderRadius: '0.7rem'
              }
            }
          }
        }
      }
    }),
    primaryColors:{
      rose: primitive['red']
    },
    surfaceColors:{
      zinc: {
        0: '#ffffff',
        50: '#fafafa',
        100: '#f4f4f5',
        200: '#e4e4e7',
        300: '#d4d4d8',
        400: '#a1a1aa',
        500: '#71717a',
        600: '#52525b',
        700: '#3f3f46',
        800: '#27272a',
        900: '#18181b',
        950: '#09090b'
      }
    }
  }
];
