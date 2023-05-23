module.exports = {
    root: true,
    env: {
        es6: true,
        node: true,
        browser: true,
    },
    parserOptions: {
        project: "./tsconfig.eslint.json",
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        tsconfigRootDir: __dirname,
    },
    plugins: ["only-error"],
    extends: ["@neolution-ch/eslint-config-neolution"],
    rules: {
        // Need to be changed in .editorconfig
        "@typescript-eslint/indent": "off",
        "linebreak-style": ["error", "windows"],

        // Electron stuff (move to inline disable?)
        "global-require": "off",
        "@typescript-eslint/no-require-imports": "off",
        "@typescript-eslint/no-var-requires": "off",
        "import/no-unresolved": "off",

        // TODO:
        "@typescript-eslint/naming-convention": "off",
        "@typescript-eslint/lines-around-comment": "off",
        "@typescript-eslint/no-namespace": "off",
        "@typescript-eslint/no-floating-promises": "off",
        "@typescript-eslint/no-unused-vars": "off",
        "@typescript-eslint/no-unsafe-enum-comparison": "off",
        "import/prefer-default-export": "off",
        "operator-linebreak": "off",
        "max-lines": "off",
        "no-prototype-builtins": "off",
        "no-restricted-syntax": "off",
    },
};
