/** @type {import('tailwindcss').Config} */
import { fontFamily } from 'tailwindcss/defaultTheme'

export const content = ['./src/ToDont**/*.liquid']
export const theme = {
  extend: {
    fontFamily: {
      sans: ['Inter var', ...fontFamily.sans],
    },
  },
}
export const variants = {}
export const plugins = [
  require('@tailwindcss/typography'),
  require('@tailwindcss/aspect-ratio'),
]
