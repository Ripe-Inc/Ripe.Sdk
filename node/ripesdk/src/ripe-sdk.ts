import fetch from 'node-fetch'
import https from 'https'

/** 
 * The RipeSdk<T> caches your ripe configuration on every 'hydrate()' call based on the value of 'options.cacheExpiry'. 
 * This object should be kept alive using a state or global variable and used to get your configuration.
 */
export class RipeSdk<T> {
  private options: RipeOptions;
  private cachedData: RipeConfig<T>;
  private expiry: number;
  private httpClient: https.Agent

  constructor(options: RipeOptions) {
    this.options = options;
    if (this.options.cacheExpiry == null || this.options.cacheExpiry < 0) {
      this.options.cacheExpiry = 300;
    }

    if (this.options.version == null) {
      this.options.version = "";
    }

    this.httpClient = new https.Agent({ keepAlive: true });
  }

  /**
  * Returns the Ripe configuration wrapped in RipeConfig<T>. This configuration object is cached based on the options.cacheExpiry value
  */
  public async hydrate(): Promise<RipeConfig<T>> {
    if (this.cachedData == null || this.expiry <= Date.now()) {
      this.cachedData = await this.fetch();
    }
    return this.cachedData;
  }

  private async fetch(): Promise<RipeConfig<T>> {
    const response = await fetch(this.options.uri, {
      method: 'POST',
      agent: this.httpClient,
      headers: { 'content-type': 'application/json', 'x-ripe-key': this.options.key },
      body: JSON.stringify({
        version: this.options.version,
        schema: this.options.schema
      })
    });

    var date = new Date();
    date.setSeconds(date.getSeconds() + this.options.cacheExpiry!);
    this.expiry = date.getUTCDate();

    return { data: (await response.json()).Data } as RipeConfig<T>;
  }
}

/** A data wrapper around your <T> config */
export type RipeConfig<T> = {
  data: T
}

/** The options for configuring your Ripe Sdk */
export type RipeOptions = {
  /** The URI pointing to your Ripe Environment */
  uri: string,
  /** The key for your Ripe Environment */
  key: string,
  /** Optional: The version of your application */
  version?: string,
  /** The number of seconds should the config be cached for */
  cacheExpiry?: number,
  /** The list of keys to request from your Ripe Environment */
  schema: string[]
}