// File: src/UI/src/types/assets.d.ts
// Purpose: Make TS understand non-code asset imports used by webpack.

declare module "*.scss";
declare module "*.css";

declare module "*.svg" { const url: string; export default url; }
declare module "*.png" { const url: string; export default url; }
declare module "*.jpg" { const url: string; export default url; }
declare module "*.gif" { const url: string; export default url; }
