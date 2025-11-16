// types/react-jss-override.d.ts
import 'react-jss';
import type {
    Styles,
    CreateUseStylesOptions,
    DefaultTheme
} from 'react-jss';

declare module 'react-jss' {
    export function createUseStyles<
        C extends string = string,
        Props = unknown,
        Theme = DefaultTheme
    >(
        styles:
            | Styles<C, Props, Theme>
            | ((theme: Theme) => Styles<C, Props, undefined>),
        options?: CreateUseStylesOptions<Theme>
    ): (data?: Props & { theme?: Theme }) => {
        [K in C]: string;
    };
}
