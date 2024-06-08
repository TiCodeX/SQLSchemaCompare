/**
 * Represent the SQL scripts of a specific item
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class CompareResultItemScripts {
    /**
     * Gets or sets the creation script of the source item
     */
    public SourceCreateScript?: string;

    /**
     * Gets or sets the creation script of the target item
     */
    public TargetCreateScript?: string;

    /**
     * Gets or sets the alter script
     */
    public AlterScript?: string;
}
