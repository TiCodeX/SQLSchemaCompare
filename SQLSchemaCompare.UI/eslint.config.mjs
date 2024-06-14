import tseslint from "typescript-eslint";
import { getBaseConfig } from "../BaseEslintConfig.mjs";

const baseConfig = await getBaseConfig(import.meta.dirname);

baseConfig.find(x => x.name === "global-ignores").ignores.push(
  "wwwroot/**/*.js",
  "wwwroot/lib",
);

export default tseslint.config(
  ...baseConfig,
  {
    name: "SQLSchemaCompare.UI",
    files: ["**/*.ts", "**/*.tsx"],
    rules: {
      "unicorn/filename-case": ["error", {
        "case": "pascalCase"
      }],
    },
  }
);
