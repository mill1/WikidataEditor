namespace WikidataEditor.Models
{
    public class StatementsOnHumans
    {
        /// <summary>
        /// Statements regarding instance (human = 'Q5')
        /// </summary>
        public Statement[] P31 { get; set; }
        /// <summary>
        /// Statements regarding place of birth
        /// </summary>
        public Statement[] P19 { get; set; }
        /// <summary>
        /// Statements regarding place of death
        /// </summary>
        public Statement[] P20 { get; set; }
        /// <summary>
        /// Statements regarding sex or gender
        /// </summary>
        public Statement[] P21 { get; set; }
        /// <summary>
        /// Statements regarding WorldCat Identities ID (superseded)
        /// </summary>
        public Statement[] P7859 { get; set; }
        /// <summary>
        /// Statements regarding VIAF ID
        /// </summary>
        public Statement[] P214 { get; set; }
        /// <summary>
        /// Statements regarding Library of Congress authority ID
        /// </summary>
        public Statement[] P244 { get; set; }
        /// <summary>
        /// Statements regarding ISNI 
        /// </summary>
        public Statement[] P213 { get; set; }
        /// <summary>
        /// Statements regarding NUKAT ID
        /// </summary>
        public Statement[] P1207 { get; set; }
        /// <summary>
        /// Statements regarding date of birth
        /// </summary>
        public Statement[] P569 { get; set; }
        /// <summary>
        /// Statements regarding date of death
        /// </summary>
        public Statement[] P570 { get; set; }
        /// <summary>
        /// Statements regarding occupation 
        /// </summary>
        public Statement[] P106 { get; set; }
        /// <summary>
        /// Statements regarding Google Knowledge Graph ID
        /// </summary>
        public Statement[] P2671 { get; set; }
        /// <summary>
        /// Statements regarding given name
        /// </summary>
        public Statement[] P735 { get; set; }
        /// <summary>
        /// Statements regarding National Library of Israel J9U ID
        /// </summary>
        public Statement[] P8189 { get; set; }
        /// <summary>
        /// Statements regarding family name
        /// </summary>
        public Statement[] P734 { get; set; }
        /// <summary>
        /// Statements regarding country of citizenship
        /// </summary>
        public Statement[] P27 { get; set; }
    }
}
