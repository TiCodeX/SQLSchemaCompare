import tseslint from "typescript-eslint";
import { getBaseConfig } from "../BaseEslintConfig.mjs";

const baseConfig = await getBaseConfig(import.meta.dirname);

export default tseslint.config(
  ...baseConfig,
  {
    name: "SQLSchemaCompare",
    rules: {
      "unicorn/prefer-module": "off",
    },
  }
);
